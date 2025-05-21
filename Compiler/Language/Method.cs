using Compiler.Interfaces;

namespace Compiler.Language;

public class Method(string name, IExpression<object>[] @params) : IInstruction
{
    public string Name { get; } = name;
    public IExpression<object>[] Params { get; } = @params;

    public void Excute(Context context)
    {
        object[] values = new object[Params.Length];
        for (int i = 0; i < values.Length; i++)
            values[i] = Params[i].Excute(context);
        // return context.Actions[Name](values);
    }
}
