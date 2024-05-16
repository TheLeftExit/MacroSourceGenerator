using System.Collections.Immutable;
using System.Text.RegularExpressions;

public static partial class RegexUtils
{
    // RemoveComments
    [GeneratedRegex(@"/\*(.*?)\*/|//(.*?)\r?\n|""((\\[^\n]|[^""\n])*)""|@(""[^""]*"")+", RegexOptions.Singleline)]
    public static partial Regex CommentsUnfiltered();
    public static string RemoveComments(string text)
    {
        return CommentsUnfiltered().Replace(text, m =>
        {
            if (m.ValueSpan.StartsWith("/*") || m.ValueSpan.StartsWith("//"))
            {
                if (m.ValueSpan.StartsWith("//")) return "\r\n";
                var lines = m.Value.Split("\r\n").Length - 1;
                return string.Concat(Enumerable.Repeat("\r\n", lines));
            }
            return m.Value;
        });
    }

    // CoalesceWhitespaces
    [GeneratedRegex(@"(\r\n[\s-[\r\n]]*)|\ +")]
    private static partial Regex NewLinesAndSpaces();
    public static string CoalesceWhitespaces(string text)
    {
        return NewLinesAndSpaces().Replace(text, m =>
        {
            if (m.ValueSpan.Contains("\r\n", StringComparison.Ordinal))
                return "\r\n";
            else
                return " ";
        });
    }

    // RemoveBackslashedNewLines
    [GeneratedRegex(@"((?:\\\r\n.*)+)(?<!\\)\r\n")]
    private static partial Regex BackslashedNewLines();
    public static string RemoveBackslashedNewLines(string text)
    {
        return BackslashedNewLines().Replace(text, m =>
        {
            var substring = m.Groups[1].Value;
            var backslashesCount = substring.Count(x => x == '\n') + 1;
            return substring.Replace("\\\r\n", "") + string.Concat(Enumerable.Repeat("\r\n", backslashesCount));
        });
    }

    // Tokenize
    [GeneratedRegex(@"\s+|(?<whitespaces>\w+)|\.\.\.|>>=|<<=|\?:|##|@#|>>|>=|==|-=|<=|<<|\+=|\+\+|\|=|\|\||\^=|\[\]|::|/=|\*=|\&=|&&|%=|!=|--|#|\>|=|\<|\+|~|\||\^|/|,|\*|&|%|!|\-|\(|\)|(?<unknown>[^\s\w]+)")]
    private static partial Regex Tokens();
    public static string[] Tokenize(string text)
    {
        return Tokens().Matches(text).Select(x => x.Value).ToArray();
    }

    [GeneratedRegex(@"^[\w]+$")]
    private static partial Regex Word();
    public static bool IsWord(ReadOnlySpan<char> token) => Word().IsMatch(token);

    [GeneratedRegex(@"^[\w-[0-9]]\w*$")]
    private static partial Regex Symbol();
    public static bool IsSymbol(ReadOnlySpan<char> token) => Symbol().IsMatch(token);
}