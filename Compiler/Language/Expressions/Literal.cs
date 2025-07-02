using Compiler.Interfaces;

namespace Compiler.Language.Expressions;

public class Literal(DinamicType value, Location location) : IExpression
{
    public  DinamicType Value { get; } = value;
    public Location Location { get; } = location;

    public  DinamicType Excute(Context context)
    {
        return Value;
    }
}