
using Compiler.Interfaces;

namespace Compiler.Language;

public class Assign(string name, IExpression value, Location location) : IInstruction
{
    public string Name { get; } = name;
    public IExpression Value { get; } = value;
    public Location Location { get; } = location;

    public void Excute(Context context)
    {
        //  De momento no hay que poner condicion para errores
        context.Variables[Name] = Value.Excute(context)!;
    }
}