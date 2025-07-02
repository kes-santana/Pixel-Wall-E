using Compiler.Interfaces;

namespace Compiler.Language.Expressions;

public class FunctionMethod(string name, IExpression[] @params, Location location) : IExpression, IInstruction
{
    public string Name { get; } = name;
    public IExpression[] Params { get; } = @params;
    public Location Location { get; } = location;

    public DinamicType Excute(Context context)
    {
        object?[] values = new object[Params.Length];
        for (int i = 0; i < values.Length; i++)
            values[i] = Params[i].Excute(context).dinamicValue;
        return new DinamicType(context.ContextFunction.CallFunction(Name, values, Location));
    }

    void IInstruction.Excute(Context context) => Excute(context);


}