// See https://aka.ms/new-console-template for more information
using Forth;

Console.WriteLine("Forth REPL - type BYE to exit");
var interp = new ForthInterpreter();

while (true)
{
    Console.Write("> ");
    var line = Console.ReadLine();
    if (line is null) break;
    try
    {
        var keepGoing = await interp.EvalAsync(line);
        Console.WriteLine(" ok");
        if (!keepGoing) break;
    }
    catch (ForthException ex)
    {
        Console.WriteLine($"Error: {ex.Code}: {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}
