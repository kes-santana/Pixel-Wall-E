using Compiler.Interfaces;

namespace Compiler.Language.Expressions;

public class Variable<T>(string name) : IExpression<T>
{
    public string Name { get; } = name;

    public T Excute(Context context)
    {
        return (T)context.Variables[Name];
    }
}
