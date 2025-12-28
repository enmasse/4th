using Forth.Core.Interpreter;
using System;
using System.IO;
using System.Linq;

var forth = new ForthInterpreter();

string? FindTtester()
{
    var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (dir != null)
    {
        var candidate = Path.Combine(dir.FullName, "tests", "ttester.4th");
        if (File.Exists(candidate)) return candidate;
        dir = dir.Parent;
    }
    return null;
}

var path = FindTtester();
if (path == null)
{
    Console.WriteLine("Could not find ttester.4th");
    return;
}

Console.WriteLine($"Loading: {path}");
var contents = File.ReadAllText(path);
await forth.EvalAsync(contents);

Console.WriteLine($"Stack after loading ttester.4th: [{string.Join(", ", forth.Stack)}]");
Console.WriteLine($"Stack count: {forth.Stack.Count}");

// Now execute #ERRORS @
await forth.EvalAsync("#ERRORS @");

Console.WriteLine($"Stack after '#ERRORS @': [{string.Join(", ", forth.Stack)}]");
Console.WriteLine($"Stack count: {forth.Stack.Count}");
