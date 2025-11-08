namespace Forth;

public interface IForthInterpreter
{
    bool Interpret(string line);
    System.Threading.Tasks.Task<bool> InterpretAsync(string line);
    // Expose stack as objects; tests expect longs so helper for casting could be added later
    IReadOnlyList<object> Stack { get; }
}