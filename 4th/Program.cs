// See https://aka.ms/new-console-template for more information
using Forth.Core;
using Forth.Core.Interpreter;
using System.IO;

Console.WriteLine("Forth REPL - type BYE to exit");
var interp = new ForthInterpreter();

if (args.Length > 0)
{
    var path = args[0];
    if (!File.Exists(path))
    {
        Console.WriteLine($"Script not found: {path}");
        return;
    }

    var lines = await File.ReadAllLinesAsync(path);
    foreach (var rawLine in lines)
    {
        var line = rawLine?.Trim();
        if (string.IsNullOrEmpty(line)) continue;

        try
        {
            var keepGoing = await interp.EvalAsync(line);
            Console.WriteLine(" ok");
            if (!keepGoing) break;
        }
        catch (ForthException ex)
        {
            Console.WriteLine($"Error: {ex.Code}: {ex.Message}");
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            break;
        }
    }

    return;
}

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
