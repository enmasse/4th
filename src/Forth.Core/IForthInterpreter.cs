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
    System.Threading.Tasks.Task<bool> EvalAsync(string line);
    /// <summary>
    /// Current parameter stack. Top of stack is the last element. Holds boxed numeric values and Task objects.
    /// </summary>
    IReadOnlyList<object> Stack { get; }
    /// <summary>Push a value onto the parameter stack.</summary>
    void Push(object value);
    /// <summary>Pop and return the top value from the parameter stack.</summary>
    object Pop();
    /// <summary>Return the top value without removing it.</summary>
    object Peek();
    /// <summary>Register a new synchronous word implemented as an Action.</summary>
    void AddWord(string name, Action<IForthInterpreter> body);
    /// <summary>Register a new asynchronous word implemented as a Func returning Task.</summary>
    void AddWordAsync(string name, Func<IForthInterpreter, System.Threading.Tasks.Task> body);
}