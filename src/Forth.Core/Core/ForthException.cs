namespace Forth.Core;

/// <summary>
/// Error codes thrown by the Forth interpreter when encountering runtime or compile-time issues.
/// </summary>
public enum ForthErrorCode
{
    /// <summary>Unspecified error.</summary>
    Unknown = 0,
    /// <summary>Not enough items on the parameter stack for the requested operation.</summary>
    StackUnderflow = 1,
    /// <summary>The requested word is not defined in the dictionary.</summary>
    UndefinedWord = 2,
    /// <summary>Compilation or definition error while parsing ": ... ;" blocks or control words.</summary>
    CompileError = 3,
    /// <summary>Division attempted with divisor equal to zero.</summary>
    DivideByZero = 4,
    /// <summary>Invalid address or memory access in VARIABLE/!/ @ operations.</summary>
    MemoryFault = 5,
    /// <summary>Type mismatch (e.g., non-numeric value used where a number is required).</summary>
    TypeError = 6,
}

/// <summary>
/// Exception type thrown by the Forth interpreter.
/// </summary>
public sealed class ForthException : Exception
{
    /// <summary>Strongly-typed error code associated with this exception.</summary>
    public ForthErrorCode Code { get; }

    /// <summary>Create a new exception with a code and message.</summary>
    /// <param name="code">The error code classification.</param>
    /// <param name="message">Human-readable message describing the failure.</param>
    public ForthException(ForthErrorCode code, string message) : base(message)
    {
        Code = code;
    }
}
