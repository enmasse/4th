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

    [Fact]
    public async Task RawOpcodes_Branch_IncrementIfNonZero()
    {
        var f = new ForthInterpreter();
        var define = @": INCNZ IL{ 
            ldarg.0 
            callvirt ""Forth.Core.Interpreter.ForthInterpreter::Pop()""
            call ""Forth.Core.Interpreter.ForthInterpreter::ToLong(object)"" 
            stloc.s 0 
            // if value == 0 skip increment
            ldloc.s 0 
            ldc.i4.0 
            conv.i8 
            ceq 
            brtrue.s SKIP 
            ldloc.s 0 
            ldc.i4.1 
            conv.i8 
            add 
            stloc.s 0 
            SKIP: 
            ldarg.0 
            ldloc.s 0 
            box System.Int64 
            callvirt ""Forth.Core.Interpreter.ForthInterpreter::Push(object)"" 
            ret 
        }IL ;";
        Assert.True(await f.EvalAsync(define));
        Assert.True(await f.EvalAsync("0 INCNZ"));
        Assert.Equal(0L, (long)f.Stack[0]);
        Assert.True(await f.EvalAsync("5 INCNZ"));
        Assert.Equal(6L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task RawOpcodes_Loop_Sum1ToN()
    {
        var f = new ForthInterpreter();
        var define = @": SUM1TON IL{ 
            ldarg.0 
            callvirt ""Forth.Core.Interpreter.ForthInterpreter::Pop()"" 
            call ""Forth.Core.Interpreter.ForthInterpreter::ToLong(object)"" 
            stloc.s 0             // n
            ldc.i4.0 
            conv.i8 
            stloc.s 1             // acc
            LOOP: 
            ldloc.s 0 
            ldc.i4.0 
            conv.i8 
            ceq 
            brtrue.s END          // if n == 0 end
            ldloc.s 1 
            ldloc.s 0 
            add 
            stloc.s 1             // acc += n
            ldloc.s 0 
            ldc.i4.1 
            conv.i8 
            sub 
            stloc.s 0             // n--
            br.s LOOP 
            END: 
            ldarg.0 
            ldloc.s 1 
            box System.Int64 
            callvirt ""Forth.Core.Interpreter.ForthInterpreter::Push(object)"" 
            ret 
        }IL ;";
        Assert.True(await f.EvalAsync(define));
        Assert.True(await f.EvalAsync("5 SUM1TON"));
        Assert.Single(f.Stack);
        Assert.Equal(15L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task RawOpcodes_Branch_NegateIfPositive()
    {
        var f = new ForthInterpreter();
        var define = @": NEGIFPOS IL{ 
            ldarg.0 
            callvirt ""Forth.Core.Interpreter.ForthInterpreter::Pop()"" 
            call ""Forth.Core.Interpreter.ForthInterpreter::ToLong(object)"" 
            stloc.s 0 
            ldloc.s 0 
            ldc.i4.0 
            conv.i8 
            cgt                // value > 0 ?
            brfalse.s DONE      // if not positive skip
            ldloc.s 0 
            neg 
            stloc.s 0 
            DONE: 
            ldarg.0 
            ldloc.s 0 
            box System.Int64 
            callvirt ""Forth.Core.Interpreter.ForthInterpreter::Push(object)"" 
            ret 
        }IL ;";
        Assert.True(await f.EvalAsync(define));
        Assert.True(await f.EvalAsync("5 NEGIFPOS"));
        Assert.Equal(-5L, (long)f.Stack[0]);
        Assert.True(await f.EvalAsync("0 NEGIFPOS"));
        Assert.Equal(0L, (long)f.Stack[1]);
        Assert.True(await f.EvalAsync("-3 NEGIFPOS"));
        Assert.Equal(-3L, (long)f.Stack[2]);
    }
}
