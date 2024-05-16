using System.Diagnostics;

public static class TokenUtils
{
    public static bool IsWhitespace(string token) => token.All(char.IsWhiteSpace);
    public static bool IsNotWhitespace(string token) => !IsWhitespace(token);

    public static bool TryParseDirective(string[] tokens, out string directiveName, out string[] parameters)
    {
        directiveName = "";
        parameters = [];

        if (tokens.Length == 0 || tokens[0] != "#") return false;

        int i = 1;
        while (i < tokens.Length && IsWhitespace(tokens[i])) i++;
        if (i == tokens.Length) return false;
        directiveName = tokens[i++];

        while (i < tokens.Length && IsWhitespace(tokens[i])) i++;
        if (i == tokens.Length) return true;

        var j = Array.FindLastIndex(tokens, IsNotWhitespace) + 1;

        parameters = tokens[i..j];
        return true;
    }

    public static MacroDefinition ParseMacroDefinition(string[] tokens)
    {
        if (tokens.Length == 0 || IsWhitespace(tokens[0])) throw new UnreachableException();
        var name = tokens[0];

        var whitespacesAfterName = false;
        int i = 1;
        while (i < tokens.Length && IsWhitespace(tokens[i]))
        {
            i++;
            whitespacesAfterName = true;
        }
        if (i == tokens.Length) return new(name, [], []);
        if (whitespacesAfterName)
        {
            return new(name, null, tokens[i..]);
        }

        string[] parameters = [];
        if (tokens[i] == "(")
        {
            var parameterCount = tokens.Skip(i + 1).TakeWhile(x => x != ")").Count(x => x == ",") + 1;
            parameters = new string[parameterCount];
            var parameterIndex = 0;
            bool anyParameters = false;
            while (tokens[++i] != ")")
            {
                anyParameters = true;
                if (RegexUtils.IsWord(tokens[i]) || tokens[i] == "...")
                {
                    parameters[parameterIndex++] = tokens[i];

                }
            }
            if (!anyParameters)
            {
                parameterCount = 0;
            }
            if (parameterIndex != parameterCount) throw new UnreachableException();
            i++;
        }

        while (i < tokens.Length && IsWhitespace(tokens[i])) i++;
        if (i == tokens.Length) return new(name, parameters, []);
        return new(name, parameters, tokens[i..]);
    }

    public static (string FileName, IncludeType Type) ParseIncludeDirective(string[] tokens)
    {
        if (tokens.Length == 0 || IsWhitespace(tokens[0])) throw new UnreachableException();
        var kind = tokens[0] switch
        {
            "\"" => IncludeType.Quotes,
            "<" => IncludeType.AngleBrackets,
            _ => throw new UnreachableException()
        };

        string fileName;

        if (kind == IncludeType.Quotes)
        {
            var j = Array.IndexOf(tokens, "\"", 1);
            fileName = string.Concat(tokens[1..j]);
            return (fileName, kind);
        }

        int i = 1;
        while (i < tokens.Length && IsWhitespace(tokens[i])) i++;
        if (i == tokens.Length) throw new UnreachableException();
        int startIndex = i;

        while (i < tokens.Length && IsNotWhitespace(tokens[i]) && tokens[i] != ">") i++;
        if (i == tokens.Length) throw new UnreachableException();
        int endIndex = i;
        fileName = string.Concat(tokens[startIndex..endIndex]);
        return (fileName, kind);
    }

    private static bool TryExpandBracketsOrVariadicMacroOnce(string[] tokens, IFileProcessingContext context, out string[] result)
    {
        var i = Array.LastIndexOf(tokens, "(");
        if (i == -1) return TryExpandRegularMacro(tokens, context, out result);
        var j = Array.IndexOf(tokens, ")", i);
        if (j == -1) throw new UnreachableException();

        var k = i - 1;
        while (k >= 0 && IsWhitespace(tokens[k])) k--;
        if (k >= 0 && RegexUtils.IsSymbol(tokens[k]))
        {
            var macro = context.GetMacroOrEmpty(tokens[k]);
            var expansionResult = ExpandVariadicMacro(tokens[(i + 1)..j], macro);
            result = [
                ..tokens[..k],
                ..expansionResult,
                ..tokens[(j + 1)..]
            ];
        }
        else
        {
            result = [
                ..tokens[..i],
                EvaluateExpression(tokens[(i + 1)..j], context) ? "1" : "0",
                ..tokens[(j + 1)..]
            ];
        }
        return true;
    }

    private static bool TryExpandRegularMacro(string[] tokens, IFileProcessingContext context, out string[] result)
    {
        var i = Array.FindIndex(tokens, x => RegexUtils.IsSymbol(x));
        if (i == -1)
        {
            result = tokens;
            return false;
        }

        var macro = context.GetMacroOrEmpty(tokens[i]);

        if (macro.Name == "defined")
        {
            var m = i + 1;
            while (m < tokens.Length && IsWhitespace(tokens[m])) m++;
            if (m == tokens.Length || !RegexUtils.IsSymbol(tokens[m])) throw new UnreachableException();

            result = [
                ..tokens[..i],
                context.GetMacroOrEmpty(tokens[m]) != MacroDefinition.Null ? "1" : "0",
                ..tokens[(m + 1)..]
            ];
        }
        else
        {
            result = [
                ..tokens[..i],
                ..macro.ValueTokens,
                ..tokens[(i + 1)..]
            ];
        }
        return true;
    }

    private static string[] ExpandBracketsAndMacros(string[] tokens, IFileProcessingContext context)
    {
        while (TryExpandBracketsOrVariadicMacroOnce(tokens, context, out tokens)) { }
        return tokens;
    }

    private static string[] ExpandVariadicMacro(string[] tokens, MacroDefinition macro)
    {
        var parameterTokens = SplitByCommas(tokens);

        var result = new List<string>();
        foreach (var token in macro?.ValueTokens ?? ["0"])
        {
            if (token == "...") throw new NotSupportedException();
            var parameterIndex = Array.IndexOf(macro.Parameters, token);
            if (parameterIndex == -1)
            {
                result.Add(token);
            }
            else
            {
                result.AddRange(parameterTokens[parameterIndex]);
            }
        }
        return result.ToArray();
    }

    private static string[][] SplitByCommas(string[] tokens)
    {
        var commaCount = tokens.Count(x => x is ",");
        int lastCommaIndex = 0;
        var result = new string[commaCount + 1][];
        for (int i = 0; i < commaCount; i++)
        {
            var commaIndex = Array.IndexOf(tokens, ",", lastCommaIndex);
            result[i] = tokens[lastCommaIndex..commaIndex];
            lastCommaIndex = commaIndex + 1;
        }
        result[commaCount] = tokens[lastCommaIndex..];
        return result;
    }

    public static bool EvaluateExpression(string[] tokens, IFileProcessingContext context)
    {
        tokens = ExpandBracketsAndMacros(tokens, context);
        return MathUtils.EvaluateNumericExpression(tokens) != 0;
    }
}