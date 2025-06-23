using Compiler.Interfaces;

namespace Compiler.Language.Expressions;

public class Method<T>(string name, IExpression<object?>[] @params) : IExpression<T>, IInstruction
{
    public string Name { get; } = name;
    public IExpression<object?>[] Params { get; } = @params;

    public T Excute(Context context)
    {
        object?[] values = new object[Params.Length];
        for (int i = 0; i < values.Length; i++)
            values[i] = Params[i].Excute(context);
        return (T)context.ContextFunction.CallFunction(Name, values);
    }

    void IInstruction.Excute(Context context) => Excute(context);
}