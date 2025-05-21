namespace Compiler;

public class Context
{
    public Dictionary<string, object> Variables { get; set; } = [];
    public Dictionary<string, int> Labels { get; set; } = [];
    // public object Functions { get; set; }
    // public object Action { get; set; }
}