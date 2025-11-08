namespace Forth;

/// <summary>
/// Abstraction over I/O used by the Forth interpreter for printing, newlines and reading input.
/// Replaceable for tests to capture output.
/// </summary>
public interface IForthIO
{
    /// <summary>Write raw text (no newline).</summary>
    void Print(string text);
    /// <summary>Write a number using invariant formatting (no newline).</summary>
    void PrintNumber(long number);
    /// <summary>Write a newline.</summary>
    void NewLine();
    /// <summary>Read a line from input; may return null on EOF.</summary>
    string? ReadLine();
}

/// <summary>
/// Console-based implementation of <see cref="IForthIO"/>.
/// </summary>
public sealed class ConsoleForthIO : IForthIO
{
    public void Print(string text) => Console.Write(text);
    public void PrintNumber(long number) => Console.Write(number);
    public void NewLine() => Console.WriteLine();
    public string? ReadLine() => Console.ReadLine();
}
