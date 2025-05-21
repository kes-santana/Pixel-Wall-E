using Compiler.Interfaces;

namespace Compiler.Language.Expressions;

public class BinaryAritmeticExpression(IExpression<int> left, IExpression<int> right, BinaryAritmeticTypes type) : IExpression<int>
{
    public IExpression<int> Left { get; private set; } = left;
    public IExpression<int> Right { get; private set; } = right;
    public BinaryAritmeticTypes Type { get; } = type;

    public int Excute(Context context) => Type switch
    {
        BinaryAritmeticTypes.Sum => Left.Excute(context) + Right.Excute(context),
        BinaryAritmeticTypes.Resta => Left.Excute(context) - Right.Excute(context),
        BinaryAritmeticTypes.Mult => Left.Excute(context) * Right.Excute(context),
        BinaryAritmeticTypes.Div => Left.Excute(context) / Right.Excute(context),
        BinaryAritmeticTypes.Potencia => (int)Math.Pow(Left.Excute(context), Right.Excute(context)),
        BinaryAritmeticTypes.Modulo => Left.Excute(context) % Right.Excute(context),
        _ => throw new InvalidOperationException()
    };
}

public class BinaryBooleanExpression(IExpression<bool> left, IExpression<bool> right, BinaryBooleanTypes type) : IExpression<bool>
{
    public IExpression<bool> Left { get; private set; } = left;
    public IExpression<bool> Right { get; private set; } = right;
    public BinaryBooleanTypes Type { get; } = type;

    public bool Excute(Context context) => Type switch
    {
        BinaryBooleanTypes.And => Left.Excute(context) && Right.Excute(context),
        BinaryBooleanTypes.Or => Left.Excute(context) || Right.Excute(context),
        BinaryBooleanTypes.Equal => Left.Excute(context) == Right.Excute(context),
        BinaryBooleanTypes.Diferent => Left.Excute(context) != Right.Excute(context),
        _ => throw new InvalidOperationException(),
    };
}
