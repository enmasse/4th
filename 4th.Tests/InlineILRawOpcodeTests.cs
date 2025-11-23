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
        var define = @": ADD2RAW IL{ 
            ldarg.0 
            callvirt ""Forth.Core.Interpreter.ForthInterpreter::Pop()""
            call ""Forth.Core.Interpreter.ForthInterpreter::ToLong(object)"" 
            stloc.s 0
            
            ldarg.0 
            callvirt ""Forth.Core.Interpreter.ForthInterpreter::Pop()""
            call ""Forth.Core.Interpreter.ForthInterpreter::ToLong(object)"" 
            
            ldloc.s 0
            add 
            stloc.s 0 
            
            ldarg.0 
            ldloc.s 0 
            box System.Int64 
            callvirt ""Forth.Core.Interpreter.ForthInterpreter::Push(object)"" 
            ret 
        }IL ;";
        Assert.True(await f.EvalAsync(define));

        Assert.True(await f.EvalAsync("8 9 ADD2RAW"));
        Assert.Single(f.Stack);
        Assert.Equal(17L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task RawOpcodes_AddTwoNumbers_FixedSlots()
    {
        var f = new ForthInterpreter();
        var define = @": ADD2_FIXED IL{ 
            ldarg.0 
            callvirt ""Forth.Core.Interpreter.ForthInterpreter::Pop()""
            call ""Forth.Core.Interpreter.ForthInterpreter::ToLong(object)"" 
            stloc.0
            
            ldarg.0 
            callvirt ""Forth.Core.Interpreter.ForthInterpreter::Pop()""
            call ""Forth.Core.Interpreter.ForthInterpreter::ToLong(object)"" 
            stloc.1

            ldloc.1
            ldloc.0
            add
            stloc.0

            ldarg.0
            ldloc.0
            box System.Int64
            callvirt ""Forth.Core.Interpreter.ForthInterpreter::Push(object)""
            ret
        }IL ;";
        Assert.True(await f.EvalAsync(define));
        Assert.True(await f.EvalAsync("3 4 ADD2_FIXED"));
        Assert.Single(f.Stack);
        Assert.Equal(7L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task RawOpcodes_AddTwoNumbers_ShortVar()
    {
        var f = new ForthInterpreter();
        var define = @": ADD2_SHORTVAR IL{ 
            ldarg.0 
            callvirt ""Forth.Core.Interpreter.ForthInterpreter::Pop()""
            call ""Forth.Core.Interpreter.ForthInterpreter::ToLong(object)"" 
            stloc.s 0
            
            ldarg.0 
            callvirt ""Forth.Core.Interpreter.ForthInterpreter::Pop()""
            call ""Forth.Core.Interpreter.ForthInterpreter::ToLong(object)"" 
            stloc.s 1

            ldloc.s 1
            ldloc.s 0
            add
            stloc.s 0

            ldarg.0
            ldloc.s 0
            box System.Int64
            callvirt ""Forth.Core.Interpreter.ForthInterpreter::Push(object)""
            ret
        }IL ;";
        Assert.True(await f.EvalAsync(define));
        Assert.True(await f.EvalAsync("10 20 ADD2_SHORTVAR"));
        Assert.Single(f.Stack);
        Assert.Equal(30L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task RawOpcodes_AddTwoNumbers_InlineVar()
    {
        var f = new ForthInterpreter();
        var define = @": ADD2_INLINEVAR IL{ 
            ldarg.0 
            callvirt ""Forth.Core.Interpreter.ForthInterpreter::Pop()""
            call ""Forth.Core.Interpreter.ForthInterpreter::ToLong(object)"" 
            stloc 0
            
            ldarg.0 
            callvirt ""Forth.Core.Interpreter.ForthInterpreter::Pop()""
            call ""Forth.Core.Interpreter.ForthInterpreter::ToLong(object)"" 
            stloc 1

            ldloc 1
            ldloc 0
            add
            stloc 0

            ldarg.0
            ldloc 0
            box System.Int64
            callvirt ""Forth.Core.Interpreter.ForthInterpreter::Push(object)""
            ret
        }IL ;";
        Assert.True(await f.EvalAsync(define));
        Assert.True(await f.EvalAsync("7 8 ADD2_INLINEVAR"));
        Assert.Single(f.Stack);
        Assert.Equal(15L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task RawOpcodes_PushLiteralValue()
    {
        var f = new ForthInterpreter();
        var define = @": PUSH123RAW IL{ 
            ldarg.0 
            ldc.i4.s 123 
            conv.i8 
            box System.Int64 
            callvirt ""Forth.Core.Interpreter.ForthInterpreter::Push(object)"" 
            ret 
        }IL ;";
        Assert.True(await f.EvalAsync(define));

        Assert.True(await f.EvalAsync("PUSH123RAW"));
        Assert.Single(f.Stack);
        Assert.Equal(123L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task RawOpcodes_PopThenPush_Roundtrip()
    {
        var f = new ForthInterpreter();
        var define = @": POPPUSH IL{ 
            ldarg.0 
            call ""Forth.Core.Interpreter.ForthInterpreter::Pop()"" 
            stloc.s 0 
            ldarg.0 
            ldloc.s 0 
            call ""Forth.Core.Interpreter.ForthInterpreter::Push(object)"" 
            ret 
        }IL ;";
        Assert.True(await f.EvalAsync(define));
        Assert.True(await f.EvalAsync("42 POPPUSH"));
        Assert.Single(f.Stack);
        Assert.Equal(42L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task RawOpcodes_PopThenPush_Roundtrip_StackApi()
    {
        var f = new ForthInterpreter();
        var define = @": POPPUSH_STACK IL{ 
            ldarg.1 
            call ""Forth.Core.Interpreter.ForthStack::Pop()"" 
            stloc.s 0 
            ldarg.1 
            ldloc.s 0 
            call ""Forth.Core.Interpreter.ForthStack::Push(object)"" 
            ret 
        }IL ;";
        Assert.True(await f.EvalAsync(define));
        Assert.True(await f.EvalAsync("99 POPPUSH_STACK"));
        Assert.Single(f.Stack);
        Assert.Equal(99L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task RawOpcodes_IncrementValue()
    {
        var f = new ForthInterpreter();
        var define = @": INC1RAW IL{ 
            ldarg.0 
            callvirt ""Forth.Core.Interpreter.ForthInterpreter::Pop()""
            call ""Forth.Core.Interpreter.ForthInterpreter::ToLong(object)"" 
            ldc.i4.1 
            conv.i8 
            add 
            stloc.s 0 
            ldarg.0 
            ldloc.s 0 
            box System.Int64 
            callvirt ""Forth.Core.Interpreter.ForthInterpreter::Push(object)"" 
            ret 
        }IL ;";
        Assert.True(await f.EvalAsync(define));

        Assert.True(await f.EvalAsync("41 INC1RAW"));
        Assert.Single(f.Stack);
        Assert.Equal(42L, (long)f.Stack[0]);
    }
}
