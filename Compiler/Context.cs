using Compiler.Language;

namespace Compiler;

public class Context(IContextFunction contextFunction, IContextAction contextAction)
{
    public Dictionary<string, DinamicType> Variables { get; set; } = [];
    public Dictionary<string, int> Labels { get; set; } = [];
    public IContextFunction ContextFunction { get; } = contextFunction;
    public IContextAction ContextAction { get; } = contextAction;
    public string? TargetLabel { get; set; }
    public bool Jump { get; set; }
}

public interface IContextFunction
{
    object CallFunction(string Name, object?[] @params, Location location);
}

public interface IContextAction
{
    void CallAction(string Name, object?[] @params, Location location);
}