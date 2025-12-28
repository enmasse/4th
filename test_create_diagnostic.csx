// Quick diagnostic test for CREATE issue
var forth = new Forth.Core.Interpreter.ForthInterpreter();

await forth.EvalAsync(": TESTER 10 20 CREATE DUMMY 30 ;");

Console.WriteLine("After defining TESTER:");
Console.WriteLine($"Dictionary has DUMMY: {forth.GetAllWordNames().Contains("DUMMY", StringComparer.OrdinalIgnoreCase)}");
Console.WriteLine($"Dictionary has TESTER: {forth.GetAllWordNames().Contains("TESTER", StringComparer.OrdinalIgnoreCase)}");

await forth.EvalAsync("TESTER");

Console.WriteLine($"\nStack after executing TESTER: [{string.Join(", ", forth.Stack)}]");
Console.WriteLine($"Stack count: {forth.Stack.Count}");
