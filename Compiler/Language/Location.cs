namespace Compiler.Language;

public record struct Location(int Row, int StartColumn, int EndColumn)
{
    public override readonly string ToString() => $"{Row}, {StartColumn}:{EndColumn}";
}