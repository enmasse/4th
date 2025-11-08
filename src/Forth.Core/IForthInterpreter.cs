namespace Forth;

/// <summary>
/// Public surface for the Forth interpreter. Interpret executes a single line of Forth source.
/// The parameter stack is exposed for test inspection as a list of objects.
/// </summary>
public interface IForthInterpreter
{
    /// <summary>
    /// Interpret a line asynchronously (awaits any async words such as AWAIT). Returns false if exit requested.
    /// </summary>
    System.Threading.Tasks.Task<bool> InterpretAsync(string line);
    /// <summary>
    /// Current parameter stack. Top of stack is the last element. Holds boxed numeric values and Task objects.
    /// </summary>
    IReadOnlyList<object> Stack { get; }
}