using Compiler.Interfaces;

namespace Compiler.Language.Expressions;

public class Variable(string name, Location location) : IExpression
{
    public string Name { get; } = name;
    public Location Location { get; } = location;
    public DinamicType Excute(Context context)
    {
        if (!context.Variables.TryGetValue(Name, out DinamicType? value))
            throw new InvalidOperationException($"La variable {Name} no existe en el contexto actual. {Location}");
        return value;
    }
}
