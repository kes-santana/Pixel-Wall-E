using System.Text.RegularExpressions;

namespace Compiler.Tokenizador;

public class Tokenizador
{
    public List<Exception> tokenException = [];
    #region Patterns

    public static readonly string id = @"[a-zA-Z][a-zA-Z0-9_]*";
    public readonly string label = $"\r?\n{id}\r?\n";
    public readonly string num = @"\d+";
    public readonly string str = @"""[^""]*""";
    public readonly string otherOp = @"\*\*";
    public readonly string comp = @"[<>=\!]=";
    public readonly string assign = @"<-";
    public readonly string op = @"[<>=%+\-\*/&\|\!\(\)\[\]\,]";
    // public readonly string other = @"^.{1}";
    #endregion

    public string GetAllPatterns()
    {
        return string.Join('|', @"[\t ]+", label, id, str, num, otherOp, assign, comp, op, "\r\n|\n", ".");
    }

    public Token[] Tokenizar(string code)
    {
        var pattern = GetAllPatterns();
        MatchCollection matches = Regex.Matches(code, pattern);

        List<Token> tokens = [];
        int line = 0;
        int col = 0;
        foreach (Match match in matches.Cast<Match>())
        {
            var value = match.Value;
            var type = GetTokenType(value, col);
            if (type is not TokenType.EndLine && string.IsNullOrEmpty(value.Trim()))
            {
                col += match.Length;
                continue;
            }
            if (type is TokenType.Error)
            {
                int actualCol = col;
                col += match.Length;
                tokenException.Add(new Exception($"Error en {line}, {actualCol}: El caracter '{value}' no es valido"));
                continue;
            }
            if (type is TokenType.Label)
                value = value.TrimEnd();
            tokens.Add(new(value, type, line, col));
            col += match.Length;
            if (type is not TokenType.EndLine)
                continue;
            line++;
            col = 0;
        }

        tokens.Add(new Token("\n", TokenType.EndLine));
        return [.. tokens];
    }

    private TokenType GetTokenType(string value, int col)
    {
        switch (value)
        {
            case "GoTo":
                return TokenType.GoTo;
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
            case ">":
                return TokenType.Major;
            case "<":
                return TokenType.Minor;
            case ">=":
                return TokenType.MajorEqual;
            case "<=":
                return TokenType.MinorEqual;
            case "==":
                return TokenType.Equal;
            case "!=":
                return TokenType.Diferent;
            case "(":
                return TokenType.ParentesisAbierto;
            case ")":
                return TokenType.ParentesisCerrado;
            case "[":
                return TokenType.CorcheteAbierto;
            case "]":
                return TokenType.CorcheteCerrado;
            case ",":
                return TokenType.Coma;
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

        if (!char.IsLetterOrDigit(value[0]))
            return TokenType.Error;

        return TokenType.Identificador;
    }
}