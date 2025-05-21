using Compiler.Interfaces;

namespace Compiler.Language.Expressions;

public class Literal<T>(T value) : IExpression<T>
{
    public T Value { get; } = value;

    public T Excute(Context context)
    {
        return Value;
    }
}