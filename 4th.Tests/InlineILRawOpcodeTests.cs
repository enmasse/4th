using System.Threading.Tasks;
using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests;

public class InlineILRawOpcodeTests
{
    [Fact]
    public async Task RawOpcodes_AddTwoNumbers()
    {
        var f = new ForthInterpreter();
        // Define IL using raw opcodes:
        // ldarg.0 (stack)
        // callvirt instance object Forth.Core.Interpreter.ForthStack::Pop()
        // ldarg.1 (intr)
        // call      int64 Forth.Core.Interpreter.ForthInterpreter::ToLong(object)
        // stloc.0 (a)
        // repeat for b
        // ldloc.1; ldloc.0; add
        // ldarg.0; box int64; callvirt Push
        var define = @": ADD2RAW IL{ 
            ldarg.0 
            callvirt Forth.Core.Interpreter.ForthStack::Pop
            ldarg.1 
            call Forth.Core.Interpreter.ForthInterpreter::ToLong(object)
            stloc.0 

            ldarg.0 
            callvirt Forth.Core.Interpreter.ForthStack::Pop 
            ldarg.1 
            call Forth.Core.Interpreter.ForthInterpreter::ToLong(object)
            stloc.1 

            ldloc.1 
            ldloc.0 
            add 
            stloc.2 

            ldarg.0 
            ldloc.2 
            box System.Int64 
            callvirt Forth.Core.Interpreter.ForthStack::Push(object) 
            ret 
        }IL ;";
        Assert.True(await f.EvalAsync(define));

        Assert.True(await f.EvalAsync("8 9 ADD2RAW"));
        Assert.Single(f.Stack);
        Assert.Equal(17L, (long)f.Stack[0]);
    }
}
