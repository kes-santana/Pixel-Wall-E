using Compiler.Tokenizador;

namespace Test;

public class Program
{
    static string Dir = @"content";
    public static void Main(string[] args)
    {
        // Configuracion por defecto
        var fileName = "test.pw";

#if DEBUG
        Dir = Path.Combine(@"..\..\..\..\", Dir);
#endif

        var path = Path.Combine(Dir, fileName);
        // Cosas del proyecto Original
        var reader = new StreamReader(path);
        if (reader.EndOfStream)
            return;
        string code = reader.ReadToEnd()!;
        Token[] tokens = Tokenizador.Tokenizar(code);
    }
}