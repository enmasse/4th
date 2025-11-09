namespace Forth.Core;

/// <summary>
/// Abstraction over I/O used by the Forth interpreter for printing, newlines and reading input.
/// Replaceable for tests to capture output.
/// </summary>
public interface IForthIO
{
    /// <summary>Write raw text (no newline).</summary>
    /// <param name="text">Text to print verbatim.</param>
    void Print(string text);
    /// <summary>Write a number using invariant formatting (no newline).</summary>
    /// <param name="number">Numeric value to print.</param>
    void PrintNumber(long number);
    /// <summary>Write a newline terminator.</summary>
    void NewLine();
    /// <summary>Read a line from input; may return <c>null</c> on EOF.</summary>
    /// <returns>The line read or <c>null</c> at end of input.</returns>
    string? ReadLine();
}

/// <summary>
/// Console-based implementation of <see cref="IForthIO"/>.
/// </summary>
public sealed class ConsoleForthIO : IForthIO
{
    /// <inheritdoc />
    public void Print(string text) => Console.Write(text);
    /// <inheritdoc />
    public void PrintNumber(long number) => Console.Write(number);
    /// <inheritdoc />
    public void NewLine() => Console.WriteLine();
    /// <inheritdoc />
    public string? ReadLine() => Console.ReadLine();
}
