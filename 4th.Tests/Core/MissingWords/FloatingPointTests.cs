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

    [Fact]
    public async Task FABS_AbsoluteValue()
    {
        var forth = new ForthInterpreter();
        // Positive float
        Assert.True(await forth.EvalAsync("3.5 FABS"));
        Assert.Single(forth.Stack);
        Assert.Equal(3.5, (double)forth.Stack[0], 10);

        // Negative float
        Assert.True(await forth.EvalAsync("-2.5 FABS"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(2.5, (double)forth.Stack[1], 10);

        // Zero
        Assert.True(await forth.EvalAsync("0.0 FABS"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(0.0, (double)forth.Stack[2], 10);
    }

    [Fact]
    public async Task FLOOR_Flooring()
    {
        var forth = new ForthInterpreter();
        // Positive float
        Assert.True(await forth.EvalAsync("3.7 FLOOR"));
        Assert.Single(forth.Stack);
        Assert.Equal(3L, (long)forth.Stack[0]);

        // Negative float
        Assert.True(await forth.EvalAsync("-2.1 FLOOR"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-3L, (long)forth.Stack[1]);

        // Integer
        Assert.True(await forth.EvalAsync("5.0 FLOOR"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(5L, (long)forth.Stack[2]);
    }

    [Fact]
    public async Task FROUND_Rounding()
    {
        var forth = new ForthInterpreter();
        // Positive float
        Assert.True(await forth.EvalAsync("3.7 FROUND"));
        Assert.Single(forth.Stack);
        Assert.Equal(4L, (long)forth.Stack[0]);

        // Negative float
        Assert.True(await forth.EvalAsync("-2.3 FROUND"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-2L, (long)forth.Stack[1]);

        // Integer
        Assert.True(await forth.EvalAsync("5.0 FROUND"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(5L, (long)forth.Stack[2]);

        // Tie to even
        Assert.True(await forth.EvalAsync("2.5 FROUND"));
        Assert.Equal(4, forth.Stack.Count);
        Assert.Equal(2L, (long)forth.Stack[3]);
    }

    [Fact]
    public async Task FSIN_Sine()
    {
        var forth = new ForthInterpreter();
        // sin(0) = 0
        Assert.True(await forth.EvalAsync("0.0 FSIN"));
        Assert.Single(forth.Stack);
        Assert.Equal(0.0, (double)forth.Stack[0], 10);

        // sin(pi/2) ? 1
        Assert.True(await forth.EvalAsync("1.5707963267948966 FSIN"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(1.0, (double)forth.Stack[1], 5);

        // sin(pi) ? 0
        Assert.True(await forth.EvalAsync("3.141592653589793 FSIN"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(0.0, (double)forth.Stack[2], 10);
    }

    [Fact]
    public async Task FTAN_Tangent()
    {
        var forth = new ForthInterpreter();
        // tan(0) = 0
        Assert.True(await forth.EvalAsync("0.0 FTAN"));
        Assert.Single(forth.Stack);
        Assert.Equal(0.0, (double)forth.Stack[0], 10);

        // tan(pi/4) ? 1
        Assert.True(await forth.EvalAsync("0.7853981633974483 FTAN"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(1.0, (double)forth.Stack[1], 5);
    }

    [Fact]
    public async Task FCOS_Cosine()
    {
        var forth = new ForthInterpreter();
        // cos(0) = 1
        Assert.True(await forth.EvalAsync("0.0 FCOS"));
        Assert.Single(forth.Stack);
        Assert.Equal(1.0, (double)forth.Stack[0], 10);

        // cos(pi) ? -1
        Assert.True(await forth.EvalAsync("3.141592653589793 FCOS"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-1.0, (double)forth.Stack[1], 5);
    }

    [Fact]
    public async Task FEXP_Exponential()
    {
        var forth = new ForthInterpreter();
        // exp(0) = 1
        Assert.True(await forth.EvalAsync("0.0 FEXP"));
        Assert.Single(forth.Stack);
        Assert.Equal(1.0, (double)forth.Stack[0], 10);

        // exp(1) ? e ? 2.718
        Assert.True(await forth.EvalAsync("1.0 FEXP"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(2.718281828459045, (double)forth.Stack[1], 5);
    }

    [Fact]
    public async Task FLOG_Logarithm()
    {
        var forth = new ForthInterpreter();
        // log(1) = 0
        Assert.True(await forth.EvalAsync("1.0 FLOG"));
        Assert.Single(forth.Stack);
        Assert.Equal(0.0, (double)forth.Stack[0], 10);

        // log(e) = 1
        Assert.True(await forth.EvalAsync("2.718281828459045 FLOG"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(1.0, (double)forth.Stack[1], 5);
    }

    [Fact]
    public async Task FACOS_Arccosine()
    {
        var forth = new ForthInterpreter();
        // acos(1) = 0
        Assert.True(await forth.EvalAsync("1.0 FACOS"));
        Assert.Single(forth.Stack);
        Assert.Equal(0.0, (double)forth.Stack[0], 10);

        // acos(0) = pi/2
        Assert.True(await forth.EvalAsync("0.0 FACOS"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(1.5707963267948966, (double)forth.Stack[1], 5);
    }

    [Fact]
    public async Task FASIN_Arcsine()
    {
        var forth = new ForthInterpreter();
        // asin(0) = 0
        Assert.True(await forth.EvalAsync("0.0 FASIN"));
        Assert.Single(forth.Stack);
        Assert.Equal(0.0, (double)forth.Stack[0], 10);

        // asin(1) = pi/2
        Assert.True(await forth.EvalAsync("1.0 FASIN"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(1.5707963267948966, (double)forth.Stack[1], 5);
    }

    [Fact]
    public async Task FATAN2_Arctangent2()
    {
        var forth = new ForthInterpreter();
        // atan2(0, 1) = 0
        Assert.True(await forth.EvalAsync("0.0 1.0 FATAN2"));
        Assert.Single(forth.Stack);
        Assert.Equal(0.0, (double)forth.Stack[0], 10);

        // atan2(1, 1) = pi/4
        Assert.True(await forth.EvalAsync("1.0 1.0 FATAN2"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(0.7853981633974483, (double)forth.Stack[1], 5);
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
