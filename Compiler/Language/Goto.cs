using Compiler.Interfaces;

namespace Compiler.Language;

public class Goto(string target, IExpression<bool> cond) : IInstruction
{
    public string Target { get; } = target;
    public IExpression<bool> Cond { get; } = cond;

    public void Excute(Context context)
    {
        if (!Cond.Excute(context))
            return;
        // hacer algo
    }
}
