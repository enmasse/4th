using Forth.Core;
using Forth.Core.Interpreter;
using System;

Console.WriteLine("Manual test runner: exercise interpreter");
var f = new ForthInterpreter();
await f.EvalAsync("1 2 3");
Console.WriteLine(string.Join(',', f.Stack));

await f.EvalAsync(": SQUARE DUP * ;");
await f.EvalAsync("4 SQUARE");
Console.WriteLine(string.Join(',', f.Stack));

try
{
    await f.EvalAsync("+");
}
catch (Exception ex)
{
    Console.WriteLine("Expected error: " + ex.Message);
}
