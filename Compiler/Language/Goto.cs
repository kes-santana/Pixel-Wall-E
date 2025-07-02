using Compiler.Interfaces;

namespace Compiler.Language;

public class Goto(string target, IExpression cond, Location location) : IInstruction
{
    public string Target { get; } = target;
    public IExpression Cond { get; } = cond;
    public Location Location { get; } = location;

    public void Excute(Context context)
    {
        var cond = Cond.Excute(context).ToBool();
        if (!cond)
            return;
        if (!context.Labels.ContainsKey(Target))
            throw new InvalidOperationException($"El label {Target} no existe en el contexto actual. {Location}");
        context.TargetLabel = Target;
        context.Jump = cond;
    }
}
