using Forth.Core.Interpreter;

var asm = typeof(ForthInterpreter).Assembly;
foreach (var n in asm.GetManifestResourceNames().OrderBy(x => x))
    Console.WriteLine(n);
