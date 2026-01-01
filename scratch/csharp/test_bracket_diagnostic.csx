using Forth.Core.Interpreter;

var f = new ForthInterpreter();
var src = "0 [IF]\n  999\n[THEN]\n42";

Console.WriteLine($"Source: '{src}'");
Console.WriteLine("");

var result = await f.EvalAsync(src);
Console.WriteLine($"Result: {result}");
Console.WriteLine($"Stack count: {f.Stack.Count}");

if (f.Stack.Count > 0)
{
    Console.WriteLine($"Stack[0]: {f.Stack[0]}");
}
else
{
    Console.WriteLine("Stack is empty!");
}

// Also test the parser directly
Console.WriteLine("");
Console.WriteLine("Testing CharacterParser directly:");
var parser = new CharacterParser(src);
while (parser.ParseNext() is { } token)
{
    Console.WriteLine($"  Token: '{token}'");
}
