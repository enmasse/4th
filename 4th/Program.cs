// See https://aka.ms/new-console-template for more information
using Forth;

Console.WriteLine("Forth REPL - placeholder");
var interp = new ForthInterpreter();

while (true)
{
    Console.Write("> ");
    var line = Console.ReadLine();
    if (line is null) break;
    try
    {
        if (!interp.Interpret(line)) break;
    }
    catch (NotImplementedException)
    {
        Console.WriteLine("Interpreter not implemented yet.");
        break;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}
