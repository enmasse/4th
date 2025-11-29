using Forth.Core;
using Forth.Core.Interpreter;
using Forth.Core.Modules;
using System;
using System.IO;

Console.WriteLine("Manual test runner: exercise interpreter");
var f = new ForthInterpreter(new TestIO());

void DumpStack(string label, IForthInterpreter inst)
{
    Console.WriteLine(label + ": [" + string.Join(",", inst.Stack) + "]");
}

await f.EvalAsync("INCLUDE \"tests/forth/framework.4th\"");
await f.EvalAsync("CREATE BUF 16 ALLOT");
await f.EvalAsync("123 BUF !");
DumpStack("after store", f);
#if DEBUG
await f.EvalAsync("LAST-STORE");
DumpStack("after LAST-STORE", f);
#endif
await f.EvalAsync("BUF @");
DumpStack("after fetch", f);
await f.EvalAsync("123 =");
DumpStack("after compare", f);

Console.WriteLine("Manual test: COUNT sequence");
var fCount = new ForthInterpreter(new TestIO());
await fCount.EvalAsync("S\" hello\" COUNT SWAP DROP 5 =");
DumpStack("after COUNT compare", fCount);

Console.WriteLine("Manual test: memory-tests steps inline");
var f4 = new ForthInterpreter(new TestIO());
Directory.SetCurrentDirectory(Path.Combine(Directory.GetCurrentDirectory(), "tests", "forth"));
await f4.EvalAsync("INCLUDE \"framework.4th\"");
await f4.EvalAsync("CREATE BUF 16 ALLOT");
try
{
    var ok4 = await f4.EvalAsync("123 BUF ! BUF @ 123 = ASSERT-TRUE");
    Console.WriteLine($"inline steps ASSERT-TRUE ok: {ok4}");
}
catch (Exception ex)
{
    Console.WriteLine("inline steps threw: " + ex.Message);
}

Console.WriteLine("Manual test: define and run TEST-STORE via harness");
var f3 = new ForthInterpreter(new TestIO());
await f3.EvalAsync("INCLUDE \"tests/forth/framework.4th\"");
await f3.EvalAsync("CREATE BUF 16 ALLOT");
await f3.EvalAsync(": TEST-STORE 123 BUF ! BUF @ 123 = ASSERT-TRUE ;");
try
{
    var ok3 = await f3.EvalAsync("S\" STORE\" TEST-CASE TEST-STORE");
    Console.WriteLine($"TEST-STORE via harness: {ok3}");
}
catch (Exception ex)
{
    Console.WriteLine("Harness TEST-STORE threw: " + ex.Message);
}

Console.WriteLine("Manual test runner: include memory-tests.4th with CWD set (fresh interpreter)");
var prev = Directory.GetCurrentDirectory();
try
{
    Directory.SetCurrentDirectory(Path.Combine(prev));
    Directory.SetCurrentDirectory(Path.Combine(prev, "tests", "forth"));
    var f2 = new ForthInterpreter(new TestIO());
    var ok = await f2.EvalAsync("INCLUDE \"memory-tests.4th\"");
    Console.WriteLine($"INCLUDE result: {ok}");
}
catch (Exception ex)
{
    Console.WriteLine("INCLUDE threw: " + ex.Message);
}
finally
{
    Directory.SetCurrentDirectory(prev);
}
