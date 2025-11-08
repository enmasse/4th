using Forth;
using System;

Console.WriteLine("Manual test runner: exercise interpreter");
var f = new ForthInterpreter();
await f.InterpretAsync("1 2 3");
Console.WriteLine(string.Join(',', f.Stack));

await f.InterpretAsync(": SQUARE DUP * ;");
await f.InterpretAsync("4 SQUARE");
Console.WriteLine(string.Join(',', f.Stack));

try
{
    await f.InterpretAsync("+");
}
catch (Exception ex)
{
    Console.WriteLine("Expected error: " + ex.Message);
}
