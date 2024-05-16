using System.Diagnostics;

public interface IFileProcessingContext
{
    string FileName { get; }
    int Line { get; }
    Func<string, MacroDefinition> GetMacroOrEmpty { get; }
}

public record MacroDefinition(string Name, string[] Parameters, string[] ValueTokens)
{
    public static MacroDefinition Defined { get; } = new("defined", ["arg"], ["defined", " ", "arg"]);
    public static MacroDefinition Null { get; } = new("", null, ["0"]);

    public override string ToString()
    {
        return $"#define\t{Name}\t{(Parameters is not null ? $"({string.Join(", ", Parameters)})" : "")}\t{string.Concat(ValueTokens)}";
    }
}


public class FileProcessor : IFileProcessingContext
{
    private readonly List<string> codeTokens;
    private readonly Stack<IfScope> ifScopes;
    private readonly Dictionary<string, MacroDefinition> macroDefinitions;
    private readonly HashSet<string> pragmaOnceFiles;
    public MacroDefinition[] Macros { get; private set; }

    private string fileName;
    private int line;

    string IFileProcessingContext.FileName => fileName;
    int IFileProcessingContext.Line => line;
    Func<string, MacroDefinition> IFileProcessingContext.GetMacroOrEmpty => x => macroDefinitions.GetValueOrDefault(x) ?? MacroDefinition.Null;

    public FileProcessor()
    {
        codeTokens = new List<string>();
        ifScopes = new Stack<IfScope>();
        ifScopes.Push(new IfScope { IsTrue = true, WasTrue = true });
        pragmaOnceFiles = new HashSet<string>();
        fileName = "ROOT";

        macroDefinitions = new Dictionary<string, MacroDefinition>
        {
            { "defined", MacroDefinition.Defined }
        };
    }

    private bool EvaluateIf(string[] tokens)
    {
        if (ifScopes.Any(x => !x.IsTrue)) return false;
        return TokenUtils.EvaluateExpression(tokens, this);

    }

    public void Process(string text)
    {
        text = RegexUtils.RemoveBackslashedNewLines(text);//text.Replace("\\\r\n", "");
        text = RegexUtils.RemoveComments(text);
        text = RegexUtils.CoalesceWhitespaces(text);

        var lines = text.Split("\r\n");
        for (var line = 0; line < lines.Length; line++)
        {
            this.line = line;
            ProcessLine(lines[line]);
        }
        Macros = [.. macroDefinitions.Values];
    }

    private void ProcessFile(string fileName, IncludeType includeType)
    {
        fileName = fileName.ToLower();
        if (pragmaOnceFiles.Contains(fileName)) return;
        pragmaOnceFiles.Add(fileName);

        var prevFileName = this.fileName;
        this.fileName = fileName;
        var text = FileUtils.ReadFile(fileName, includeType);
        Process(text);
        this.fileName = prevFileName;
    }

    private void ProcessLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return;
        var tokens = RegexUtils.Tokenize(line);

        if (tokens.Length == 0) throw new UnreachableException();

        var isDirective = TokenUtils.TryParseDirective(tokens, out string directiveName, out string[] parameters);

        if (!isDirective && ifScopes.All(x => x.IsTrue))
        {
            codeTokens.AddRange(tokens);
            return;
        }

        if (directiveName == "") return;

        if (directiveName == "ifdef")
        {
            directiveName = "if";
            parameters = ["defined", parameters[0]];
        }
        else if (directiveName == "ifndef")
        {
            directiveName = "if";
            parameters = ["!", "defined", parameters[0]];
        }

        if (directiveName == "if")
        {
            ifScopes.Push(new IfScope { IsTrue = EvaluateIf(parameters) });
        }
        else if (directiveName == "elif")
        {
            var currentScope = ifScopes.Peek();
            if (currentScope.IsTrue || currentScope.WasTrue)
            {
                currentScope.IsTrue = false;
                currentScope.WasTrue = true;
            }
            else
            {
                ifScopes.Pop();
                currentScope.IsTrue = EvaluateIf(parameters);
                ifScopes.Push(currentScope);
            }
        }
        else if (directiveName == "else")
        {
            var currentScope = ifScopes.Peek();
            if (currentScope.IsTrue || currentScope.WasTrue)
            {
                currentScope.IsTrue = false;
                currentScope.WasTrue = true;
            }
            else
            {
                currentScope.IsTrue = true;
            }
        }
        else if (directiveName == "endif")
        {
            ifScopes.Pop();
        }
        else if (!ifScopes.All(x => x.IsTrue))
        {
            return;
        }
        else if (directiveName == "define")
        {
            var definition = TokenUtils.ParseMacroDefinition(parameters);
            macroDefinitions[definition.Name] = definition;
        }
        else if (directiveName == "undef")
        {
            macroDefinitions.Remove(parameters.Single());
        }
        else if (directiveName == "include")
        {
            var (fileName, includeType) = TokenUtils.ParseIncludeDirective(parameters);
            ProcessFile(fileName, includeType);
        }
    }
}

public class IfScope
{
    public bool IsTrue { get; set; }
    public bool WasTrue { get; set; }
}
