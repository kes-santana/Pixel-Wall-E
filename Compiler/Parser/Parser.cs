
using Compiler.Interfaces;
using Compiler.Language;
using Compiler.Language.Expressions;
using Compiler.Tokenizador;

namespace Compiler.Parser;

public class Parser
{
    int tokenIndex;
    public List<Exception> ParserErrors = [];

    public delegate IExpression<T>? GetExpression<T>(Token[] tokens);

    public IInstruction Parse(Token[] tokens)
    {
        tokenIndex = 0;
        return ParseBlock(tokens);
    }

    public IInstruction ParseBlock(Token[] tokens)
    {
        List<IInstruction> instructions = [];

        while (tokenIndex < tokens.Length)
        {
            IInstruction? inst = ParseAssign(tokens)
                        ?? ParseLabel(tokens)
                        ?? ParseGoto(tokens)
                        ?? ParseMethod(tokens);
            ProcessInstruction(tokens, instructions, inst);
        }

        return new Block(instructions);
    }

    private void ProcessInstruction(Token[] tokens, List<IInstruction> instructions, IInstruction? inst)
    {
        if (inst is not null)
        {
            instructions.Add(inst);
            return;
        }
        JumpInstruction(tokens);
    }

    private void JumpInstruction(Token[] tokens)
    {
        while (!TokenMatch(tokens, TokenType.EndLine))
        {
            tokenIndex++;
        }
        return;
    }

    #region Terminar
    private IInstruction? ParseMethod(Token[] tokens)
    {
        var name = tokens[tokenIndex].Value;
        List<IExpression<object>> parameters = [];
        if (!ParseMethodA(tokens, parameters))
            return default;
        IExpression<object>[] myParams = [.. parameters];
        return new Method(name, myParams);
    }

    private IExpression<T>? ParseMethodWith<T>(Token[] tokens)
    {
        var name = tokens[tokenIndex].Value;
        List<IExpression<object>> parameters = [];
        if (!ParseMethodA(tokens, parameters))
            return default;
        IExpression<object>[] myParams = [.. parameters];
        return new Method<T>(name, myParams);
    }
    private bool ParseMethodA(Token[] tokens, List<IExpression<object>> parameters)
    {
        if (!TokenMatch(tokens, TokenType.Identificador))
            return false;

        // primer parentesis
        if (!TokenMatch(tokens, TokenType.ParentesisAbierto))
        {
            ParserErrors.Add(new Exception("Se esperaba ("));
            return false;
        }
        //  parametros
        Parameters(tokens, parameters);
        // segundo parentesis
        if (!TokenMatch(tokens, TokenType.ParentesisCerrado))
        {
            ParserErrors.Add(new Exception("Se esperaba )"));
            return false;
        }
        return true;
    }

    private void Parameters(Token[] tokens, List<IExpression<object>> @params)
    {
        if (!TokenMatch(tokens, TokenType.Identificador))
        {
            if (!TokenMatch(tokens, TokenType.Num))
                if (!TokenMatch(tokens, TokenType.String))
                    return;
            object value = int.TryParse(tokens[tokenIndex - 1].Value, out int num)
                ? num : tokens[tokenIndex - 1].Value;
            @params.Add(new Literal<object>(value));
            RevisarParams(tokens, @params);
            return;
        }
        @params.Add(new Variable<object>(tokens[tokenIndex - 1].Value));
        RevisarParams(tokens, @params);
    }

    private void RevisarParams(Token[] tokens, List<IExpression<object>> @params)
    {
        var index = tokenIndex;
        if (TokenMatch(tokens, TokenType.EndLine))
        {
            tokenIndex = index;
            return;
        }
        if (TokenMatch(tokens, TokenType.ParentesisCerrado))
        {
            tokenIndex = index;
            return;
        }
        if (!TokenMatch(tokens, TokenType.Coma))
        {
            if (!TokenMatch(tokens, TokenType.Num))
                if (!TokenMatch(tokens, TokenType.String))
                    return;
            ParserErrors.Add(new Exception("Se esperaba ','"));
            return;
        }
        Parameters(tokens, @params);
    }

    private IInstruction? ParseGoto(Token[] tokens)
    {
        if (!TokenMatch(tokens, TokenType.GoTo))
            return default;

        if (!TokenMatch(tokens, TokenType.CorcheteAbierto))
        {
            ParserErrors.Add(new Exception("Se esperaba ["));
            return default;
        }

        string target = tokens[tokenIndex].Value;
        if (!TokenMatch(tokens, TokenType.Identificador))
        {
            ParserErrors.Add(new Exception("Se esperaba identificador"));
            return default;
        }

        if (!TokenMatch(tokens, TokenType.CorcheteCerrado))
        {
            ParserErrors.Add(new Exception("Se esperaba ]"));
            return default;
        }

        if (!TokenMatch(tokens, TokenType.ParentesisAbierto))
        {
            ParserErrors.Add(new Exception("Se esperaba ("));
            return default;
        }

        var cond = ParseBoolean(tokens);
        if (cond is null)
        {
            ParserErrors.Add(new Exception("Se esperaba condicion"));
            return default;
        }

        if (!TokenMatch(tokens, TokenType.ParentesisCerrado))
        {
            ParserErrors.Add(new Exception("Se esperaba )"));
            return default;
        }
        return new Goto(target, cond);
    }

    #endregion
    private IInstruction? ParseLabel(Token[] tokens)
    {
        var token = tokens[tokenIndex];
        if (!TokenMatch(tokens, TokenType.Label))
            return default;

        return new Label(token.Value, token.Row);
    }

    private IInstruction? ParseAssign(Token[] tokens)
    {
        var token = tokens[tokenIndex];
        var name = token.Value;
        if (token.Type is not TokenType.Identificador)
            return default;

        if (tokens[++tokenIndex].Type is not TokenType.Assign)
        {
            tokenIndex--;
            return default;
        }
        tokenIndex++;
        return GetAssign(tokens, name);
    }

    private IInstruction? GetAssign(Token[] tokens, string name)
    {
        int actualIndex = tokenIndex;
        IExpression<bool>? boolean = ParseBoolean(tokens);
        if (boolean is not null)
            return new Assign<bool>(name, boolean);

        tokenIndex = actualIndex;
        IExpression<int>? num = ParseNumber(tokens);
        if (num is not null)
            return new Assign<int>(name, num);

        tokenIndex = actualIndex;
        IExpression<string>? str = ParseColor(tokens);
        if (str is not null)
            return new Assign<string>(name, str);

        tokenIndex = actualIndex;
        return default;
    }

    private IExpression<string>? ParseColor(Token[] tokens)
        => ParseLiteral<string>(tokens, TokenType.String);

    #region Arithmetics

    private IExpression<int>? ParseNumber(Token[] tokens) => ParseSum(tokens);
    private IExpression<int>? ParseSum(Token[] tokens)
    {
        return ParseBuild(tokens, ParseMult, [TokenType.Suma, TokenType.Resta]);
    }

    private IExpression<int>? ParseMult(Token[] tokens)
    {
        return ParseBuild(tokens, ParsePow, [TokenType.Mult, TokenType.Module, TokenType.Div]);
    }

    private IExpression<int>? ParsePow(Token[] tokens)
    {
        return ParseBuild(tokens, ParseNumLiteral, [TokenType.Exp]);
    }

    private IExpression<int>? ParseNumLiteral(Token[] tokens)
        => ParseLiteral<int>(tokens, TokenType.Num);
    #endregion

    #region Booleans and Comparisons
    private IExpression<bool>? ParseBoolean(Token[] tokens) => ParseOr(tokens);

    private IExpression<bool>? ParseOr(Token[] tokens)
    {
        return ParseBuild(tokens, ParseAnd, [TokenType.Or]);
    }

    private IExpression<bool>? ParseAnd(Token[] tokens)
    {
        return ParseBuild(tokens, ParseBoolLiteral, [TokenType.And]);
    }

    private IExpression<bool>? Comparacion(Token[] tokens)
    {
        var left = ParseNumber(tokens);
        if (left == null)
            return default;

        var op = MatchOperator(tokens, [
            TokenType.Major, TokenType.Minor,
            TokenType.MajorEqual, TokenType.MinorEqual,
            TokenType.Equal, TokenType.Diferent,
        ]);
        if (!op.HasValue)
            return default;

        var right = ParseNumber(tokens);
        if (right == null)
            return default;
        return new ComparationBoolExpression(left, right, ToBinaryTypes(op.Value));
    }

    private IExpression<bool>? ParseBoolLiteral(Token[] tokens) =>
        ParseLiteral<bool>(tokens, TokenType.Boolean) ?? Comparacion(tokens);
    #endregion

    #region  Tools
    private IExpression<T>? ParseBuild<T>(Token[] tokens, GetExpression<T> NextLevel, TokenType[] types)
    {
        var startIndex = tokenIndex;
        var left = NextLevel(tokens);
        if (left is null)
        {
            tokenIndex = startIndex;
            return default;
        }
        return ParseBuild(tokens, left, NextLevel, types);
    }

    private IExpression<T>? ParseBuild<T>(Token[] tokens, IExpression<T> left, GetExpression<T> NextLevel, TokenType[] types)
    {
        var op = MatchOperator(tokens, types);
        if (op is null)
            return left;
        BinaryTypes type = ToBinaryTypes(op.Value);

        if (IsShift(type))
            return Shift(tokens, left, NextLevel, type, types);
        return Reduce(tokens, left, NextLevel, type, types);
    }
    private IExpression<T>? Shift<T>(Token[] tokens, IExpression<T> left, GetExpression<T> NextLevel, BinaryTypes type, TokenType[] types)
    {
        IExpression<T>? right;
        right = ParseBuild(tokens, NextLevel, types);
        if (right is null)
            return default;
        return BuildExpression<T, T>(left, right, type);
    }
    private IExpression<T>? Reduce<T>(Token[] tokens, IExpression<T> left, GetExpression<T> NextLevel, BinaryTypes type, TokenType[] types)
    {
        IExpression<T>? right;
        right = NextLevel(tokens);
        if (right is null)
            return default;
        var exp = BuildExpression<T, T>(left, right, type);
        if (exp is null)
            return default;
        return ParseBuild(tokens, exp, NextLevel, types);
    }
    private IExpression<T>? BuildExpression<T, K>(IExpression<K> left, IExpression<K> right, BinaryTypes type)
    {
        return type switch
        {
            BinaryTypes.Sum or BinaryTypes.Resta or BinaryTypes.Mult or
            BinaryTypes.Div or BinaryTypes.Potencia or BinaryTypes.Modulo
                => new BinaryAritmeticExpression(
                    (left as IExpression<int>)!,
                    (right as IExpression<int>)!,
                    type) as IExpression<T>,

            BinaryTypes.And or BinaryTypes.Or
                => new BinaryBooleanExpression(
                    (left as IExpression<bool>)!,
                    (right as IExpression<bool>)!,
                    type) as IExpression<T>,

            BinaryTypes.Equal or BinaryTypes.Diferent or
            BinaryTypes.MajorEqual or BinaryTypes.MinorEqual
                => new ComparationBoolExpression(
                    (left as IExpression<int>)!,
                    (right as IExpression<int>)!,
                    type) as IExpression<T>,
            _ => throw new NotImplementedException(),
        };
    }

    private BinaryTypes ToBinaryTypes(TokenType op)
    {
        return op switch
        {
            TokenType.Suma => BinaryTypes.Sum,
            TokenType.Resta => BinaryTypes.Resta,
            TokenType.Mult => BinaryTypes.Mult,
            TokenType.Div => BinaryTypes.Div,
            TokenType.Module => BinaryTypes.Modulo,
            TokenType.Exp => BinaryTypes.Potencia,
            _ => throw new NotImplementedException(),
        };
    }

    // que me expliquen esto
    private IExpression<T>? ParseLiteral<T>(Token[] tokens, TokenType type)
        where T : IParsable<T>
    {
        var token = tokens[tokenIndex];
        if (TokenMatch(tokens, type))
        {
            T myBool = T.Parse(token.Value, null);
            return new Literal<T>(myBool);
        }
        var exp = ParseMethodWith<T>(tokens);
        if (exp != null)
            return exp;
        if (TokenMatch(tokens, TokenType.Identificador))
            return new Variable<T>(token.Value);
        return default;
    }

    public TokenType? MatchOperator(Token[] tokens, TokenType[] types)
    {
        for (int i = 0; i < types.Length; i++)
        {
            if (TokenMatch(tokens, types[i]))
                return types[i];
        }
        return null;
    }

    private bool TokenMatch(Token[] tokens, TokenType type)
    {
        if (tokens[tokenIndex].Type != type)
            return false;
        tokenIndex++;
        return true;
    }

    private bool IsShift(BinaryTypes type)
        => type is
        BinaryTypes.Sum or
        BinaryTypes.Mult or
        BinaryTypes.Potencia or
        BinaryTypes.And or
        BinaryTypes.Or;

    #endregion
}