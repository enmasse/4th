namespace Forth;

public interface IForthInterpreter
{
    // Feed a line of input; returns true if successful, false if exit requested
    bool Interpret(string line);

    // Expose the parameter stack for testing (top is last)
    IReadOnlyList<long> Stack { get; }
}