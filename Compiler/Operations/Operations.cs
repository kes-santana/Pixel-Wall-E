class BinaryAritmeticExpression(IExpression<int> left, IExpression<int> right, BinaryAritmeticTypes type) : IExpression<int>
{
    public IExpression<int> Left { get; private set; } = left;
    public IExpression<int> Right { get; private set; } = right;
    public BinaryAritmeticTypes Type { get; } = type;

    public int Excute() => Type switch
    {
        BinaryAritmeticTypes.Sum => Left.Excute() + Right.Excute(),
        BinaryAritmeticTypes.Resta => Left.Excute() - Right.Excute(),
        BinaryAritmeticTypes.Mult => Left.Excute() * Right.Excute(),
        BinaryAritmeticTypes.Div => Left.Excute() / Right.Excute(),
        BinaryAritmeticTypes.Potencia => (int)Math.Pow(Left.Excute(), Right.Excute()),
        BinaryAritmeticTypes.Modulo => Left.Excute() % Right.Excute(),
        _ => throw new InvalidOperationException()
    };
}
class BinaryBooleanExpression(IExpression<bool> left, IExpression<bool> right, BinaryBooleanTypes type) : IExpression<bool>
{
    public IExpression<bool> Left { get; private set; } = left;
    public IExpression<bool> Right { get; private set; } = right;
    public BinaryBooleanTypes Type { get; } = type;

    public bool Excute() => Type switch
    {
        BinaryBooleanTypes.And => Left.Excute() && Right.Excute(),
        BinaryBooleanTypes.Or => Left.Excute() || Right.Excute(),
        BinaryBooleanTypes.Equal => Left.Excute() == Right.Excute(),
        BinaryBooleanTypes.Diferent => Left.Excute() != Right.Excute(),
        _ => throw new InvalidOperationException(),
    };
}
 