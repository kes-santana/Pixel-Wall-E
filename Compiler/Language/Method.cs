using Compiler.Interfaces;

namespace Compiler.Language;

public class ActionMethod(string name, IExpression[] @params, Location location) : IInstruction
{
    public string Name { get; } = name;
    public IExpression[] Params { get; } = @params;
    public Location Location { get; } = location;

    public void Excute(Context context)
    {
        object?[] values = new object[Params.Length];
        for (int i = 0; i < values.Length; i++)
            values[i] = Params[i].Excute(context).dinamicValue;
        context.ContextAction.CallAction(Name, values, Location);
    }
}
