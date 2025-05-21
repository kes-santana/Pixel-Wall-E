namespace Compiler.Interfaces;

public interface IExpression<T>
{
    public T Excute(Context context);
}
