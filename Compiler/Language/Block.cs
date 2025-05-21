using System.Collections;
using Compiler.Interfaces;

namespace Compiler.Language;

public class Block(IList<IInstruction> instructions) : IInstruction
{
    public IList<IInstruction> Instructions { get; } = instructions;

    public void Excute(Context context)
    {
        for (int i = 0; i < Instructions.Count; i++)
        {
            Instructions[i].Excute(context);
        }
    }
}