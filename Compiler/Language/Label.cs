using Compiler.Interfaces;

namespace Compiler.Language;

public class Label(string name, int row, Location location) : IInstruction
{
    public string Name { get; } = name;
    public int Row { get; } = row;
    public Location Location { get; } = location;

    public void Excute(Context context)
    {if (context.Labels.ContainsKey(Name))
            throw new Exception($"El label '{Name}' ya existe. {Location}");
        context.Labels[Name] = Row;
    }
}