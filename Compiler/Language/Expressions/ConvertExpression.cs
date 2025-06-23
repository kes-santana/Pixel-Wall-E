using Compiler.Interfaces;

namespace Compiler.Language.Expressions;

public class ConvertExpression<T>(IExpression<T> value) : IExpression<object?>
{
    private readonly IExpression<T> value = value;

    public object? Excute(Context context) => value.Excute(context);
}