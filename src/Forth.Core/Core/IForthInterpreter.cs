namespace Forth.Core;

/// <summary>
/// Public surface for the Forth interpreter. Executes single lines of Forth source code and
/// exposes the parameter stack for inspection (top of stack is the last element).
///
/// Internally uses optimized ForthValue structs for performance, but maintains object-based
/// compatibility for the public API.
/// </summary>
public interface IForthInterpreter
{
    /// <summary>
    /// Interpret a line asynchronously (awaits any async words such as <c>AWAIT</c>).
    /// </summary>
    /// <param name="line">Single line of Forth source to evaluate.</param>
    /// <returns><c>true</c> if the interpreter should continue running; <c>false</c> if an exit was requested (BYE/QUIT).</returns>
    System.Threading.Tasks.Task<bool> EvalAsync(string line);

    /// <summary>
    /// Current parameter stack (top is last element). Contains boxed numeric values, strings, tasks, and other objects pushed by words.
    /// 
    /// Note: While the public API uses object types for compatibility, the internal implementation uses typed ForthValue structs
    /// for optimal performance without boxing overhead.
    /// </summary>
    IReadOnlyList<object> Stack { get; }

    /// <summary>Push a value onto the parameter stack.</summary>
    /// <param name="value">The value to push (boxed if primitive).</param>
    void Push(object value);

    /// <summary>Pop and return the top value from the parameter stack.</summary>
    /// <returns>The top-most value.</returns>
    object Pop();

    /// <summary>Return (peek) the top value without removing it.</summary>
    /// <returns>The top-most value.</returns>
    object Peek();

    /// <summary>
    /// Register a new synchronous word.
    /// </summary>
    /// <param name="name">The Forth word name.</param>
    /// <param name="body">The action to execute when the word runs.</param>
    void AddWord(string name, Action<IForthInterpreter> body);

    /// <summary>
    /// Register a new asynchronous word.
    /// </summary>
    /// <param name="name">The Forth word name.</param>
    /// <param name="body">The asynchronous function returning a task that will be awaited.</param>
    void AddWordAsync(string name, Func<IForthInterpreter, System.Threading.Tasks.Task> body);
}
