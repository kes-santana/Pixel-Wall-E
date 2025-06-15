using Compiler.Interfaces;

namespace Compiler.Language.Expressions;

public class BinaryAritmeticExpression(IExpression<int> left, IExpression<int> right, BinaryTypes type) : IExpression<int>
{
    public IExpression<int> Left { get; private set; } = left;
    public IExpression<int> Right { get; private set; } = right;
    public BinaryTypes Type { get; } = type;

    public int Excute(Context context) => Type switch
    {
        BinaryTypes.Sum => Left.Excute(context) + Right.Excute(context),
        BinaryTypes.Resta => Left.Excute(context) - Right.Excute(context),
        BinaryTypes.Mult => Left.Excute(context) * Right.Excute(context),
        BinaryTypes.Div => Left.Excute(context) / Right.Excute(context),
        BinaryTypes.Potencia => (int)Math.Pow(Left.Excute(context), Right.Excute(context)),
        BinaryTypes.Modulo => Left.Excute(context) % Right.Excute(context),
        _ => throw new InvalidOperationException()
    };
}

public class BinaryBooleanExpression(IExpression<bool> left, IExpression<bool> right, BinaryTypes type) : IExpression<bool>
{
    public IExpression<bool> Left { get; private set; } = left;
    public IExpression<bool> Right { get; private set; } = right;
    public BinaryTypes Type { get; } = type;
 
    public bool Excute(Context context) => Type switch
    {
        BinaryTypes.And => Left.Excute(context) && Right.Excute(context),
        BinaryTypes.Or => Left.Excute(context) || Right.Excute(context),
        _ => throw new InvalidOperationException(),
    };
}

public class ComparationBoolExpression(IExpression<int> left, IExpression<int> right, BinaryTypes type) : IExpression<bool>
{
    public IExpression<int> Left { get; } = left;
    public IExpression<int> Right { get; } = right;
    public BinaryTypes Type { get; } = type;

    public bool Excute(Context context) => Type switch
    {
        BinaryTypes.Equal => Left.Excute(context) == Right.Excute(context),
        BinaryTypes.Diferent => Left.Excute(context) != Right.Excute(context),
        BinaryTypes.Major => Left.Excute(context) > Right.Excute(context),
        BinaryTypes.Minor => Left.Excute(context) < Right.Excute(context),
        BinaryTypes.MajorEqual => Left.Excute(context) >= Right.Excute(context),
        BinaryTypes.MinorEqual => Left.Excute(context) <= Right.Excute(context),
        _ => throw new InvalidOperationException(),
    };
}
 
