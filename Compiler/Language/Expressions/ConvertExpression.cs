using Compiler.Interfaces;

namespace Compiler.Language.Expressions;

public class ConvertExpression<T>(IExpression value,Location location) : IExpression
{
    private readonly IExpression value = value;
    public Location Location { get; } = location;

//  De momento no hay que poner condicion para errores
    public DinamicType Excute(Context context) => value.Excute(context);
}