using Compiler.Language;

namespace Compiler.Interfaces;

public interface IExpression
{
    public DinamicType Excute(Context context);
}
