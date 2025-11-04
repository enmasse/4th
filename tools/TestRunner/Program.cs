using Forth;
using System;

Console.WriteLine("Manual test runner: exercise interpreter");
var f = new ForthInterpreter();
f.Interpret("1 2 3");
Console.WriteLine(string.Join(',', f.Stack));

f.Interpret(": SQUARE DUP * ;");
f.Interpret("4 SQUARE");
Console.WriteLine(string.Join(',', f.Stack));

try
{
    f.Interpret("+");
}
catch (Exception ex)
{
    Console.WriteLine("Expected error: " + ex.Message);
}
