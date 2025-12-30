using Forth.Core.Interpreter;

// Test the fatan2 pattern directly
var f = new ForthInterpreter();

// Load ttester first
await f.EvalAsync(@"s"" tests/ttester.4th"" INCLUDED");

// Set verbose
await f.EvalAsync("true verbose !");

// Check verbose value
await f.EvalAsync("verbose @ .");
Console.WriteLine($"verbose = {f.Pop()}");

// Try the pattern from lines 86-92
var testCode = @"
verbose @ [IF]
:noname  ( -- fp.separate? )
  depth >r 1e depth >r fdrop 2r> = ; execute
cr .( floating-point and data stacks )
[IF] .( *separate*) [ELSE] .( *not separate*) [THEN]
cr
[THEN]

testing normal values
";

Console.WriteLine("About to evaluate test code...");
Console.WriteLine($"_isCompiling before = {f._isCompiling}");

try {
    await f.EvalAsync(testCode);
    Console.WriteLine("Test passed!");
} catch (Exception ex) {
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"_isCompiling after error = {f._isCompiling}");
}
