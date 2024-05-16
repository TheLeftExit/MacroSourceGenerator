public class MathUtils
{
    public static int EvaluateNumericExpression(string[] tokens)
    {
        tokens = tokens.Where(TokenUtils.IsNotWhitespace).ToArray();
        while (tokens.Length > 1 && EvaluateOperandOnce(tokens, out tokens)) { }
        return ParseNumericToken(tokens.Single());
    }

    private static bool EvaluateOperandOnce(string[] tokens, out string[] result)
    {
        if (TryEvaluateUnary(tokens, "!", x => x == 0 ? 1 : 0, out result)) return true;
        if (TryEvaluateBinary(tokens, ">>", (x1, x2) => x1 >> x2, out result)) return true;
        if (TryEvaluateBinary(tokens, "<", (x1, x2) => (x1 < x2) ? 1 : 0, out result)) return true;
        if (TryEvaluateBinary(tokens, ">", (x1, x2) => (x1 > x2) ? 1 : 0, out result)) return true;
        if (TryEvaluateBinary(tokens, "<=", (x1, x2) => (x1 <= x2) ? 1 : 0, out result)) return true;
        if (TryEvaluateBinary(tokens, ">=", (x1, x2) => (x1 >= x2) ? 1 : 0, out result)) return true;
        if (TryEvaluateBinary(tokens, "==", (x1, x2) => (x1 == x2) ? 1 : 0, out result)) return true;
        if (TryEvaluateBinary(tokens, "!=", (x1, x2) => (x1 != x2) ? 1 : 0, out result)) return true;
        if (TryEvaluateBinary(tokens, "&", (x1, x2) => x1 & x2, out result)) return true;
        if (TryEvaluateBinary(tokens, "|", (x1, x2) => x1 | x2, out result)) return true;
        if (TryEvaluateBinary(tokens, "&&", (x1, x2) => (x1 != 0 && x2 != 0) ? 1 : 0, out result)) return true;
        if (TryEvaluateBinary(tokens, "||", (x1, x2) => (x1 != 0 || x2 != 0) ? 1 : 0, out result)) return true;

        throw new NotImplementedException();
    }

    private static bool TryEvaluateUnary(string[] tokens, string token, Func<int, int> evaluate, out string[] result)
    {
        if (TryFindIndexBeforeValue(tokens, token, true, out var i))
        {
            result = [
                ..tokens[..i],
                evaluate(ParseNumericToken(tokens[i + 1])).ToString(),
                ..tokens[(i + 2)..]
            ];
            return true;
        }
        result = tokens;
        return false;
    }

    private static bool TryEvaluateBinary(string[] tokens, string token, Func<int, int, int> evaluate, out string[] result)
    {
        if (TryFindIndexBeforeValue(tokens, token, false, out var i))
        {
            result = [
                ..tokens[..(i - 1)],
                evaluate(ParseNumericToken(tokens[i - 1]), ParseNumericToken(tokens[i + 1])).ToString(),
                ..tokens[(i + 2)..]
            ];
            return true;
        }
        result = tokens;
        return false;
    }

    private static int ParseNumericToken(string token)
    {
        token = token.Trim('L', 'l');
        if (token.StartsWith("0x"))
        {
            return int.Parse(token[2..], System.Globalization.NumberStyles.HexNumber);
        }
        return int.Parse(token);
    }

    private static bool TryFindIndexBeforeValue(string[] tokens, string token, bool isUnary, out int result)
    {
        for (int i = isUnary ? 0 : 1; i < tokens.Length - 1; i++)
        {
            var ok = tokens[i] == token
                && RegexUtils.IsWord(tokens[i + 1])
                && (isUnary || RegexUtils.IsWord(tokens[i - 1]));
            if (ok)
            {
                result = i;
                return true;
            }
        }
        result = -1;
        return false;
    }
}