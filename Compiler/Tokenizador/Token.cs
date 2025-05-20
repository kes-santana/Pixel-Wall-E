namespace Compiler.Tokenizador;

public class Token(string value, TokenType type, int row = 0, int col = 0)
{
    public string Value { get; set; } = value;
    public TokenType Type { get; set; } = type;
    public int Row { get; set; } = row;
    public int Col { get; set; } = col;
}
