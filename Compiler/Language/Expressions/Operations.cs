using Compiler.Interfaces;

namespace Compiler.Language.Expressions;

public class UnaryAritmeticExpression(IExpression exp, UnaryType type) : IExpression
{
    public IExpression Exp { get; } = exp;
    public UnaryType Type { get; } = type;

    public DinamicType Excute(Context context)
    {
        var value = Exp.Excute(context);
        return Type switch
        {
            UnaryType.Negativo when value.dinamicValue is int num => new DinamicType(-num),
            _ => throw new InvalidOperationException($"No se puede convertir a {Type} un {value.ConvertToTokenType()}"),
        };
    }
}

public class BinaryAritmeticExpression(IExpression left, IExpression right, BinaryTypes type) : IExpression
{
    public IExpression Left { get; private set; } = left;
    public IExpression Right { get; private set; } = right;
    public BinaryTypes Type { get; } = type;

    public DinamicType Excute(Context context)
    {
        var left = Left.Excute(context);
        var right = Right.Excute(context);
        if (left.type != right.type)
        {
            var leftType = left.ConvertToTokenType();
            var rightType = right.ConvertToTokenType();
            throw new InvalidOperationException($"No se puede hacer la operacion {Type} entre {leftType} y {rightType}");
        }

        return Type switch
        {
            BinaryTypes.Sum => new DinamicType(left.ToInt() + right.ToInt()),
            BinaryTypes.Resta => new DinamicType(left.ToInt() - right.ToInt()),
            BinaryTypes.Mult => new DinamicType(left.ToInt() * right.ToInt()),
            BinaryTypes.Div => new DinamicType(left.ToInt() / right.ToInt()),
            BinaryTypes.Potencia => new DinamicType((int)Math.Pow(left.ToInt(), right.ToInt())),
            BinaryTypes.Modulo => new DinamicType(left.ToInt() % right.ToInt()),
            _ => throw new InvalidOperationException()
        };
    }
}

public class BinaryBooleanExpression(IExpression left, IExpression right, BinaryTypes type) : IExpression
{
    public IExpression Left { get; private set; } = left;
    public IExpression Right { get; private set; } = right;
    public BinaryTypes Type { get; } = type;

    public DinamicType Excute(Context context)
    {
        if (Left.Excute(context).type != Right.Excute(context).type)
            throw new InvalidOperationException($"No se puede hacer la operacion {Type} entre {Left} y {Right}");

        return Type switch
        {
            BinaryTypes.And => new DinamicType(Left.Excute(context).ToBool() && Right.Excute(context).ToBool()),
            BinaryTypes.Or => new DinamicType(Left.Excute(context).ToBool() || Right.Excute(context).ToBool()),
            _ => throw new InvalidOperationException(),
        };
    }
}

public class ComparationBoolExpression(IExpression left, IExpression right, BinaryTypes type) : IExpression
{
    public IExpression Left { get; } = left;
    public IExpression Right { get; } = right;
    public BinaryTypes Type { get; } = type;

    public DinamicType Excute(Context context)
    {
        if (Left.Excute(context).type != Right.Excute(context).type)
            throw new InvalidOperationException($"No se puede hacer la operacion {Type} entre {Left} y {Right}");

        return Type switch
        {
            BinaryTypes.Equal => new DinamicType(Left.Excute(context).ToInt() == Right.Excute(context).ToInt()),
            BinaryTypes.Diferent => new DinamicType(Left.Excute(context).ToInt() != Right.Excute(context).ToInt()),
            BinaryTypes.Major => new DinamicType(Left.Excute(context).ToInt() > Right.Excute(context).ToInt()),
            BinaryTypes.Minor => new DinamicType(Left.Excute(context).ToInt() < Right.Excute(context).ToInt()),
            BinaryTypes.MajorEqual => new DinamicType(Left.Excute(context).ToInt() >= Right.Excute(context).ToInt()),
            BinaryTypes.MinorEqual => new DinamicType(Left.Excute(context).ToInt() <= Right.Excute(context).ToInt()),
            _ => throw new InvalidOperationException(),
        };
    }
}

