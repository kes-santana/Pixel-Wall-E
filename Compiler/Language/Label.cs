using Compiler.Interfaces;

namespace Compiler.Language;

public class Label(string name, int row) : IInstruction
{
    public string Name { get; } = name;
    public int Row { get; } = row;

    public void Excute(Context context)
    {
        context.Labels[Name] = Row;
    }
}