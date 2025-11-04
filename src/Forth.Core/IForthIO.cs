namespace Forth;

public interface IForthIO
{
    void Print(string text);
    void PrintNumber(long number);
    void NewLine();
    string? ReadLine();
}

public sealed class ConsoleForthIO : IForthIO
{
    public void Print(string text) => Console.Write(text);
    public void PrintNumber(long number) => Console.Write(number);
    public void NewLine() => Console.WriteLine();
    public string? ReadLine() => Console.ReadLine();
}
