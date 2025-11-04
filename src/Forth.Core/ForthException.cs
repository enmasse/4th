namespace Forth;

public enum ForthErrorCode
{
    Unknown = 0,
    StackUnderflow = 1,
    UndefinedWord = 2,
    CompileError = 3,
    DivideByZero = 4,
    MemoryFault = 5,
}

public sealed class ForthException : Exception
{
    public ForthErrorCode Code { get; }

    public ForthException(ForthErrorCode code, string message) : base(message)
    {
        Code = code;
    }
}
