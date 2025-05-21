
using Compiler.Interfaces;

namespace Compiler.Language;

public class Assign<T>(string name, IExpression<T> value) : IInstruction
{
    public string Name { get; } = name;
    public IExpression<T> Value { get; } = value;

    public void Excute(Context context)
    {
        context.Variables[Name] = Value.Excute(context)!;
    }
}