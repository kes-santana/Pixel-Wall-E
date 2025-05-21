using Compiler.Interfaces;
using Compiler.Language;
using Compiler.Tokenizador;

namespace Compiler.Parser;

public class Parser
{
    int tokenIndex;

    public IInstruction Parse(Token[] tokens)
    {
        tokenIndex = 0;
        return ParseBlock(tokens);
    }

    public IInstruction ParseBlock(Token[] tokens)
    {
        List<IInstruction> instructions = [];

        do
        {
            IInstruction? inst = ParseAssign(tokens)
                        ?? ParseLabel(tokens)
                        ?? ParseGoto(tokens)
                        ?? ParseMethod(tokens);
            ProcessInstruction(tokens, instructions, inst);
        } while (tokenIndex < tokens.Length);

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
        throw new NotImplementedException();
    }

    private IInstruction? ParseMethod(Token[] tokens)
    {
        throw new NotImplementedException();
    }

    private IInstruction? ParseGoto(Token[] tokens)
    {
        throw new NotImplementedException();
    }

    private IInstruction? ParseLabel(Token[] tokens)
    {
        var token = tokens[tokenIndex];
        if (!TokenMatch(tokens, TokenType.Label))
            return default;
        return new Label(token.Value, token.Row);
    }

    private bool TokenMatch(Token[] tokens, TokenType type)
    {
        if (tokens[tokenIndex].Type != type)
            return false;
        tokenIndex++;
        return true;
    }

    private IInstruction? ParseAssign(Token[] tokens)
    {
        var token = tokens[tokenIndex];
        var name = token.Value;
        if (token.Type is not TokenType.Identificador)
            return default;
        tokenIndex++;
        if (tokens[tokenIndex].Type is not TokenType.Assign)
            return default;
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
        return default;
    }

    private IExpression<bool>? ParseBoolean(Token[] tokens)
    {
        throw new NotImplementedException();
    }

    private IExpression<int>? ParseNumber(Token[] tokens)
    {
        throw new NotImplementedException();
    }


}