using Compiler.Tokenizador;

namespace Compiler.Language
{
    public class DinamicType(object value)
    {
        public Type? type = value?.GetType();
        public object? dinamicValue = value;

        public int ToInt()
        {
            if (dinamicValue is int num)
                return num;
            throw new Exception("Este objeto no se puede castear a int");
        }
        public override string ToString()
        {
            if (dinamicValue is string str)
                return str;
            throw new Exception("Este objeto no se puede castear a string");
        }
        public bool ToBool()
        {
            if (dinamicValue is bool boolean)
                return boolean;
            throw new Exception("Este objeto no se puede castear a bool");
        }

        public static bool TryParse(string value, TokenType type, out DinamicType? dinamic)
        {
            dinamic = type switch
            {
                TokenType.Boolean => new DinamicType(bool.Parse(value)),
                TokenType.String => new DinamicType(value),
                TokenType.Num => new DinamicType(int.Parse(value)),
                _ => null
            };
            return dinamic is not null;
        }

        public TokenType ConvertToTokenType()
        {
            if (type == typeof(int))
                return TokenType.Num;
            if (type == typeof(string))
                return TokenType.String;
            if (type == typeof(bool))
                return TokenType.Boolean;
            throw new InvalidOperationException();
        }
    }
}