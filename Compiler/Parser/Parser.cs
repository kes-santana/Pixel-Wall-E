
using Compiler.Interfaces;
using Compiler.Language;
using Compiler.Language.Expressions;
using Compiler.Tokenizador;

namespace Compiler.Parser;

public class Parser
{
    int tokenIndex;
    public List<Exception> ParserErrors = [];

    public delegate IExpression? GetExpression(Token[] tokens);

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
        if (inst is not null && ParserErrors.Count == 0)
        {
            instructions.Add(inst);
            return;
        }
        JumpInstruction(tokens);
    }

    private void JumpInstruction(Token[] tokens)
    {
        // TODO: si el error es general, poner un exception aqui que diga que la linea dio error
        // Sabes que es un error cuando lo que viene no es un cambio de linea
        while (!TokenMatch(tokens, TokenType.EndLine))
        {
            tokenIndex++;
        }
        return;
    }

    #region Terminar
    private IInstruction? ParseMethod(Token[] tokens)
    {
        var token = tokens[tokenIndex];
        List<IExpression> parameters = [];
        if (!TokenMatch(tokens, TokenType.Identificador) || !TokenMatch(tokens, TokenType.ParentesisAbierto) || !ParseMethodA(tokens, parameters))
            return default;
        IExpression[] myParams = [.. parameters];
        var location = new Location()
        {
            Row = token.Row + 1,
            StartColumn = token.Col,
            EndColumn = tokens[tokenIndex - 1].Col
        };
        return new ActionMethod(token.Value, myParams, location);
    }
    private IExpression? ParseMethodWith(Token[] tokens)
    {
        int startIndex = tokenIndex;
        var token = tokens[tokenIndex];
        List<IExpression> parameters = [];
        if (!TokenMatch(tokens, TokenType.Identificador) || !TokenMatch(tokens, TokenType.ParentesisAbierto))
        {
            tokenIndex = startIndex;
            return default;
        }
        if (!ParseMethodA(tokens, parameters))
            return default;
        IExpression[] myParams = [.. parameters];
        var location = new Location()
        {
            Row = token.Row + 1,
            StartColumn = token.Col,
            EndColumn = tokens[tokenIndex - 1].Col,
        };
        return new FunctionMethod(token.Value, myParams, location);
    }
    private bool ParseMethodA(Token[] tokens, List<IExpression> parameters)
    {
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

    private void Parameters(Token[] tokens, List<IExpression> @params)
    {
        var startIndex = tokenIndex;
        if (!TokenMatch(tokens, TokenType.Identificador))
        {
            var num = ParseNumber(tokens);

            if (num is not null)
            {
                var location1 = new Location()
                {
                    Row = tokens[startIndex].Row + 1,
                    StartColumn = tokens[startIndex].Col,
                    EndColumn = tokens[tokenIndex - 1].Col,
                };
                @params.Add(new ConvertExpression<int>(num, location1));
                RevisarParams(tokens, @params);
                return;
            }

            if (!TokenMatch(tokens, TokenType.String))
                return;
            var location2 = new Location()
            {
                Row = tokens[startIndex].Row + 1,
                StartColumn = tokens[startIndex].Col,
                EndColumn = tokens[tokenIndex - 1].Col,
            };
            var value = new DinamicType(tokens[tokenIndex - 1].Value);
            @params.Add(new Literal(value, location2));
            RevisarParams(tokens, @params);
            return;
        }
        if (TokenMatch(tokens, TokenType.ParentesisAbierto))
        {
            tokenIndex -= 2;
            var exp = ParseMethodWith(tokens);
            if (exp is not null)
            {
                @params.Add(exp);
                RevisarParams(tokens, @params);
                return;
            }
        }
        var location3 = new Location()
        {
            Row = tokens[startIndex].Row + 1,
            StartColumn = tokens[startIndex].Col,
            EndColumn = tokens[tokenIndex - 1].Col,
        };
        @params.Add(new Variable(tokens[tokenIndex - 1].Value, location3));
        RevisarParams(tokens, @params);
    }
    private void RevisarParams(Token[] tokens, List<IExpression> @params)
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
            ParserErrors.Add(new Exception("Se esperaba ','"));
            return;
        }
        Parameters(tokens, @params);
    }
    private IInstruction? ParseGoto(Token[] tokens)
    {
        int startIndex = tokenIndex;
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

        var location = new Location()
        {
            Row = tokens[startIndex].Row + 1,
            StartColumn = tokens[startIndex].Col,
            EndColumn = tokens[tokenIndex - 1].Col
        };
        return new Goto(target, cond, location);
    }

    #endregion
    private IInstruction? ParseLabel(Token[] tokens)
    {
        var token = tokens[tokenIndex];
        if (!TokenMatch(tokens, TokenType.Label))
            return default;
        var location = new Location()
        {
            Row = token.Row + 1,
            StartColumn = token.Col,
            EndColumn = tokens[tokenIndex - 1].Col
        };
        return new Label(token.Value, token.Row, location);
    }
    private IInstruction? ParseAssign(Token[] tokens)
    {
        int actualIndex = tokenIndex;
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
        var exp = GetAssign(tokens, name);
        if (exp is not null)
            return exp;
        tokenIndex = actualIndex;
        return default;
    }
    private IInstruction? GetAssign(Token[] tokens, string name)
    {
        int actualIndex = tokenIndex;
        IExpression? num = ParseNumber(tokens);
        if (TokenMatch(tokens, TokenType.EndLine))
        {
            if (num is null)
                return default;
            var location = new Location()
            {
                Row = tokens[tokenIndex - 2].Row,
                StartColumn = tokens[tokenIndex - 2].Col,
                EndColumn = tokens[tokenIndex - 1].Col
            };
            return new Assign(name, num, location);
        }

        tokenIndex = actualIndex;
        IExpression? str = ParseColor(tokens);
        if (TokenMatch(tokens, TokenType.EndLine))
        {
            if (str is null)
                return default;
            var location = new Location()
            {
                Row = tokens[tokenIndex - 2].Row,
                StartColumn = tokens[tokenIndex - 2].Col,
                EndColumn = tokens[tokenIndex - 1].Col
            };
            return new Assign(name, str, location);
        }

        tokenIndex = actualIndex;
        IExpression? boolean = ParseBoolean(tokens);
        if (TokenMatch(tokens, TokenType.EndLine))
        {
            if (boolean is null)
                return default;
            var location = new Location()
            {
                Row = tokens[tokenIndex - 2].Row,
                StartColumn = tokens[tokenIndex - 2].Col,
                EndColumn = tokens[tokenIndex - 1].Col
            };
            return new Assign(name, boolean, location);
        }

        var location1 = new Location()
        {
            Row = tokens[tokenIndex - 2].Row,
            StartColumn = tokens[tokenIndex - 2].Col,
            EndColumn = tokens[tokenIndex - 1].Col
        };
        tokenIndex = actualIndex;

        ParserErrors.Add(new Exception($"Debe asignar un valor a la variable '{name}'. {location1}"));
        return default;
    }

    private IExpression? ParseColor(Token[] tokens)
        => ParseLiteral(tokens);

    #region Arithmetics

    private IExpression? ParseNumber(Token[] tokens) => ParseSum(tokens);
    private IExpression? ParseSum(Token[] tokens)
    {
        return ParseBuild(tokens, ParseMult, [TokenType.Suma, TokenType.Resta]);
    }
    private IExpression? ParseMult(Token[] tokens)
    {
        return ParseBuild(tokens, ParsePow, [TokenType.Mult, TokenType.Module, TokenType.Div]);
    }
    private IExpression? ParsePow(Token[] tokens)
    {
        return ParseBuild(tokens, ParseResta, [TokenType.Exp]);
    }
    // private IExpression? ParseNumLiteral(Token[] tokens)
    //     => ParseResta(tokens);
    #endregion

    #region Booleans and Comparisons
    private IExpression? ParseBoolean(Token[] tokens) => ParseOr(tokens);

    private IExpression? ParseOr(Token[] tokens)
    {
        return ParseBuild(tokens, ParseAnd, [TokenType.Or]);
    }

    private IExpression? ParseAnd(Token[] tokens)
    {
        return ParseBuild(tokens, ParseBoolLiteral, [TokenType.And]);
    }

    private IExpression? Comparacion(Token[] tokens)
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

    private IExpression? ParseBoolLiteral(Token[] tokens) =>
        Comparacion(tokens) ?? ParseLiteral(tokens);
    #endregion

    #region  Tools
    private IExpression? ParseBuild(Token[] tokens, GetExpression NextLevel, TokenType[] types)
    {
        // Este metodo construye el nodo buscando un left y rigth
        var startIndex = tokenIndex;
        var left = NextLevel(tokens);
        if (left is null)
            return default;
        return ParseBuild(tokens, left, NextLevel, types);
    }

    private IExpression? ParseBuild(Token[] tokens, IExpression left, GetExpression NextLevel, TokenType[] types)
    {
        // Este metodo construye el nodo a partir de un left y buscando un rigth
        var op = MatchOperator(tokens, types);
        if (op is null)
            return left;
        BinaryTypes type = ToBinaryTypes(op.Value);

        if (IsShift(type))
            return Shift(tokens, left, NextLevel, type, types);
        return Reduce(tokens, left, NextLevel, type, types);
    }
    private IExpression? Shift(Token[] tokens, IExpression left, GetExpression NextLevel, BinaryTypes type, TokenType[] types)
    {
        IExpression? right;
        right = ParseBuild(tokens, NextLevel, types);
        if (right is null)
            return default;
        return BuildExpression(left, right, type);
    }
    private IExpression? Reduce(Token[] tokens, IExpression left, GetExpression NextLevel, BinaryTypes type, TokenType[] types)
    {
        IExpression? right;
        right = NextLevel(tokens);
        if (right is null)
            return default;
        var exp = BuildExpression(left, right, type);
        if (exp is null)
            return default;
        return ParseBuild(tokens, exp, NextLevel, types);
    }
    private IExpression? BuildExpression(IExpression left, IExpression right, BinaryTypes type)
    {
        return type switch
        {
            BinaryTypes.Sum or BinaryTypes.Resta or BinaryTypes.Mult or
            BinaryTypes.Div or BinaryTypes.Potencia or BinaryTypes.Modulo
                => new BinaryAritmeticExpression(left, right, type),

            BinaryTypes.And or BinaryTypes.Or
                => new BinaryBooleanExpression(left, right, type),

            BinaryTypes.Equal or BinaryTypes.Diferent or
            BinaryTypes.MajorEqual or BinaryTypes.MinorEqual
                => new ComparationBoolExpression(left, right, type),
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
            TokenType.And => BinaryTypes.And,
            TokenType.Or => BinaryTypes.Or,
            TokenType.Major => BinaryTypes.Major,
            TokenType.Minor => BinaryTypes.Minor,
            TokenType.MajorEqual => BinaryTypes.MajorEqual,
            TokenType.MinorEqual => BinaryTypes.MinorEqual,
            TokenType.Equal => BinaryTypes.Equal,
            TokenType.Diferent => BinaryTypes.Diferent,
            _ => throw new NotImplementedException(),
        };
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
    private IExpression? ParseLiteral(Token[] tokens)
    {
        var token = tokens[tokenIndex];
        if (DinamicType.TryParse(token.Value, token.Type, out var value))
        {
            tokenIndex++;
            var location = new Location()
            {
                Row = token.Row + 1,
                StartColumn = token.Col,
                EndColumn = tokens[tokenIndex - 1].Col
            };
            return new Literal(value!, location);
        }
        var exp = ParseMethodWith(tokens);
        if (exp != null)
            return exp;
        if (TokenMatch(tokens, TokenType.Identificador))
        {
            var location = new Location()
            {
                Row = token.Row + 1,
                StartColumn = token.Col,
                EndColumn = tokens[tokenIndex - 1].Col
            };
            return new Variable(token.Value, location);
        }
        return default;
    }
    public IExpression? ParseResta(Token[] tokens)
    {
        int count = 0;
        while (TokenMatch(tokens, TokenType.Resta))
        {
            count++;
        }
        if (count % 2 == 0)
            return ParseLiteral(tokens);
        return ParseLiteralNegativo(tokens);
    }

    private IExpression? ParseLiteralNegativo(Token[] tokens)
    {
        IExpression? exp;
        var token = tokens[tokenIndex];
        bool myBool = DinamicType.TryParse(token.Value, token.Type, out var value);
        if (myBool)
        {
            tokenIndex++;
            var location1 = new Location()
            {
                Row = token.Row + 1,
                StartColumn = token.Col,
                EndColumn = tokens[tokenIndex - 1].Col,
            };
            exp = new Literal(value!, location1);
            return new UnaryAritmeticExpression(exp, UnaryType.Negativo);
        }
        exp = ParseMethodWith(tokens);
        var location2 = new Location()
        {
            Row = token.Row + 1,
            StartColumn = token.Col,
            EndColumn = tokens[tokenIndex - 1].Col,
        };
        if (exp is null && TokenMatch(tokens, TokenType.Identificador))
            exp = new Variable(token.Value, location2);
        if (exp is not null)
            return new UnaryAritmeticExpression(exp, UnaryType.Negativo);
        return default;
    }
}