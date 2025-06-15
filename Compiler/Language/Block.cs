using System.Collections;
using Compiler.Interfaces;

namespace Compiler.Language;

public class Block(IList<IInstruction> instructions) : IInstruction
{
    public IList<IInstruction> Instructions { get; } = instructions;

    public void Excute(Context context)
    {
        SearchLabels(context);
        for (int i = 0; i < Instructions.Count; i++)
        {
            if (Instructions[i] is Label)
                continue;
            Instructions[i].Excute(context);
            if (context.Jump)
            {
                context.Jump = false;
                i = context.Labels[context.TargetLabel!];
            }
        }
    }

    public void SearchLabels(Context context)
    {
        foreach (var item in Instructions)
        {
            if (item is Label)
                item.Excute(context);
        }
    }
}