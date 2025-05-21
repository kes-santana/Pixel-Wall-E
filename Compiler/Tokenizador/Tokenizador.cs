using System.Text.RegularExpressions;

namespace Compiler.Tokenizador;

public static class Tokenizador
{
    #region Patterns

    public static readonly string id = @"[a-zA-Z][a-zA-Z0-9\_]*";
    public static readonly string label = $"{id}\r?\n";
    public static readonly string num = @"\d+";
    public static readonly string str = @"""[^""]*""";
    public static readonly string otherOp = @"\*\*";
    public static readonly string comp = @"[<>=\!]=";
    public static readonly string assign = @"<-";
    public static readonly string op = @"[<>=%+\-\*/&\|\!\(\),]";
    #endregion

    public static string GetAllPatterns()
    {
        return string.Join('|', @"[\t ]+", label, id, str, num, otherOp, assign, comp, op, "\r?\n");
    }

    public static Token[] Tokenizar(string code)
    {
        var pattern = GetAllPatterns();
        MatchCollection matches = Regex.Matches(code, pattern);

        List<Token> tokens = [];
        int line = 0;
        int col = 0;
        foreach (Match match in matches.Cast<Match>())
        {
            var value = match.Value;
            var type = GetTokenType(value);
            tokens.Add(new(value, type, line, col));
            col += match.Length;
            if (type != TokenType.EndLine)
                continue;
            line++;
            col = 0;
        }

        return [.. tokens];
    }

    private static TokenType GetTokenType(string value)
    {
        switch (value)
        {
            case "+":
                return TokenType.Suma;
            case "-":
                return TokenType.Resta;
            case "/":
                return TokenType.Div;
            case "*":
                return TokenType.Mult;
            case "%":
                return TokenType.Module;
            case "**":
                return TokenType.Exp;
            case "<-":
                return TokenType.Assign;
            case "&&":
                return TokenType.And;
            case "||":
                return TokenType.Or;
            case "\r\n":
            case "\n":
                return TokenType.EndLine;
        }

        if (int.TryParse(value, out int _))
            return TokenType.Num;
        if (bool.TryParse(value, out bool _))
            return TokenType.Boolean;
        if (value[^1] == '\n')
            return TokenType.Label;
        if (value[^1] == '"' && value[0] == '"')
            return TokenType.String;
        return TokenType.Identificador;
    }
}