using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class FloatingPointTests
{
    [Fact]
    public async Task FloatingPoint_Arithmetic()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1.5 2.5 F+"));
        Assert.Single(forth.Stack);
        var v = forth.Stack[0];
        Assert.IsType<double>(v);
        Assert.Equal(4.0, (double)v, 10);

        // Other ops
        Assert.True(await forth.EvalAsync("5.5 2.0 F-"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(3.5, (double)forth.Stack[^1], 10);

        Assert.True(await forth.EvalAsync("3.0 4.0 F*"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(12.0, (double)forth.Stack[^1], 10);

        Assert.True(await forth.EvalAsync("9.0 3.0 F/"));
        Assert.Equal(4, forth.Stack.Count);
        Assert.Equal(3.0, (double)forth.Stack[^1], 10);

        Assert.True(await forth.EvalAsync("2.5 FNEGATE"));
        Assert.Equal(5, forth.Stack.Count);
        Assert.Equal(-2.5, (double)forth.Stack[^1], 10);
    }

    [Fact]
    public async Task FloatingPoint_Variables_And_Constants()
    {
        var forth = new ForthInterpreter();
        // FVARIABLE and F@/F!
        Assert.True(await forth.EvalAsync("FVARIABLE FV"));
        Assert.True(await forth.EvalAsync("0.0 FV F!"));
        Assert.True(await forth.EvalAsync("FV F@"));
        Assert.Single(forth.Stack);
        Assert.Equal(0.0, (double)forth.Stack[0], 10);

        // FCONSTANT
        Assert.True(await forth.EvalAsync("3.14159 FCONSTANT PI"));
        Assert.True(await forth.EvalAsync("PI"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(3.14159, (double)forth.Stack[^1], 5);
    }

    [Fact]
    public async Task FloatingPoint_Printing()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync("2.5 F."));
        // F. writes text using IO
        Assert.Single(io.Outputs);
        Assert.Contains("2.5", io.Outputs[0]);
    }

    [Fact]
    public async Task FloatingPoint_Comparisons()
    {
        var forth = new ForthInterpreter();
        // F0=
        Assert.True(await forth.EvalAsync("0.0 F0="));
        Assert.Single(forth.Stack);
        Assert.Equal(-1L, (long)forth.Stack[0]); // true

        Assert.True(await forth.EvalAsync("1.0 F0="));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[1]); // false

        // F0<
        Assert.True(await forth.EvalAsync("-1.0 F0<"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[2]); // true

        Assert.True(await forth.EvalAsync("1.0 F0<"));
        Assert.Equal(4, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[3]); // false

        // F<
        Assert.True(await forth.EvalAsync("1.0 2.0 F<"));
        Assert.Equal(5, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[4]); // true

        Assert.True(await forth.EvalAsync("2.0 1.0 F<"));
        Assert.Equal(6, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[5]); // false

        // F=
        Assert.True(await forth.EvalAsync("1.5 1.5 F="));
        Assert.Equal(7, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[6]); // true

        Assert.True(await forth.EvalAsync("1.5 2.0 F="));
        Assert.Equal(8, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[7]); // false
    }

    [Fact]
    public async Task FToS_ConvertsFloatToInt()
    {
        var forth = new ForthInterpreter();
        // Positive float
        Assert.True(await forth.EvalAsync("3.7 F>S"));
        Assert.Single(forth.Stack);
        Assert.Equal(3L, (long)forth.Stack[0]);

        // Negative float
        Assert.True(await forth.EvalAsync("-2.9 F>S"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-2L, (long)forth.Stack[1]);

        // Zero
        Assert.True(await forth.EvalAsync("0.0 F>S"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[2]);
    }

    private sealed class TestIO : IForthIO
    {
        public readonly System.Collections.Generic.List<string> Outputs = new();
        public void Print(string text) => Outputs.Add(text);
        public void PrintNumber(long number) => Outputs.Add(number.ToString());
        public void NewLine() => Outputs.Add("\n");
        public string? ReadLine() => null;
    }
}
