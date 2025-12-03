using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System;

namespace Forth.Tests.Core.MissingWords;

public class FloatingPointTests
{
    [Fact]
    public async Task FloatingPoint_Arithmetic()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1.5d 2.5d F+"));
        Assert.Single(forth.Stack);
        var v = forth.Stack[0];
        Assert.IsType<double>(v);
        Assert.Equal(4.0, (double)v, 10);

        // Other ops
        Assert.True(await forth.EvalAsync("5.5d 2.0d F-"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(3.5, (double)forth.Stack[^1], 10);

        Assert.True(await forth.EvalAsync("3.0d 4.0d F*"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(12.0, (double)forth.Stack[^1], 10);

        Assert.True(await forth.EvalAsync("9.0d 3.0d F/"));
        Assert.Equal(4, forth.Stack.Count);
        Assert.Equal(3.0, (double)forth.Stack[^1], 10);

        Assert.True(await forth.EvalAsync("2.5d FNEGATE"));
        Assert.Equal(5, forth.Stack.Count);
        Assert.Equal(-2.5, (double)forth.Stack[^1], 10);
    }

    [Fact]
    public async Task FloatingPoint_Variables_And_Constants()
    {
        var forth = new ForthInterpreter();
        // FVARIABLE and F@/F!
        Assert.True(await forth.EvalAsync("FVARIABLE FV"));
        Assert.True(await forth.EvalAsync("0.0d FV F!"));
        Assert.True(await forth.EvalAsync("FV F@"));
        Assert.Single(forth.Stack);
        Assert.Equal(0.0, (double)forth.Stack[0], 10);

        // FCONSTANT
        Assert.True(await forth.EvalAsync("3.14159d FCONSTANT PI"));
        Assert.True(await forth.EvalAsync("PI"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(3.14159, (double)forth.Stack[^1], 5);
    }

    [Fact]
    public async Task FloatingPoint_Printing()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync("2.5d F."));
        // F. writes text using IO
        Assert.Single(io.Outputs);
        Assert.Contains("2.5", io.Outputs[0]);
    }

    [Fact]
    public async Task FloatingPoint_Comparisons()
    {
        var forth = new ForthInterpreter();
        // F0=
        Assert.True(await forth.EvalAsync("0.0d F0="));
        Assert.Single(forth.Stack);
        Assert.Equal(-1L, (long)forth.Stack[0]); // true

        Assert.True(await forth.EvalAsync("1.0d F0="));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[1]); // false

        // F0<
        Assert.True(await forth.EvalAsync("-1.0d F0<"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[2]); // true

        Assert.True(await forth.EvalAsync("1.0d F0<"));
        Assert.Equal(4, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[3]); // false

        // F<
        Assert.True(await forth.EvalAsync("1.0d 2.0d F<"));
        Assert.Equal(5, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[4]); // true

        Assert.True(await forth.EvalAsync("2.0d 1.0d F<"));
        Assert.Equal(6, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[5]); // false

        // F=
        Assert.True(await forth.EvalAsync("1.5d 1.5d F="));
        Assert.Equal(7, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[6]); // true

        Assert.True(await forth.EvalAsync("1.5d 2.0d F="));
        Assert.Equal(8, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[7]); // false
    }

    [Fact]
    public async Task FloatingPoint_NewComparisons()
    {
        var forth = new ForthInterpreter();
        // F>
        Assert.True(await forth.EvalAsync("2.0d 1.0d F>"));
        Assert.Single(forth.Stack);
        Assert.Equal(-1L, (long)forth.Stack[0]); // true

        Assert.True(await forth.EvalAsync("1.0d 2.0d F>"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[1]); // false

        // F!=
        Assert.True(await forth.EvalAsync("1.5d 2.0d F!="));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[2]); // true

        Assert.True(await forth.EvalAsync("1.5d 1.5d F!="));
        Assert.Equal(4, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[3]); // false

        // F<=
        Assert.True(await forth.EvalAsync("1.0d 2.0d F<="));
        Assert.Equal(5, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[4]); // true

        Assert.True(await forth.EvalAsync("2.0d 1.0d F<="));
        Assert.Equal(6, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[5]); // false

        Assert.True(await forth.EvalAsync("1.5d 1.5d F<="));
        Assert.Equal(7, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[6]); // true

        // F>=
        Assert.True(await forth.EvalAsync("2.0d 1.0d F>="));
        Assert.Equal(8, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[7]); // true

        Assert.True(await forth.EvalAsync("1.0d 2.0d F>="));
        Assert.Equal(9, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[8]); // false

        Assert.True(await forth.EvalAsync("1.5d 1.5d F>="));
        Assert.Equal(10, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[9]); // true

        // F0>
        Assert.True(await forth.EvalAsync("1.0d F0>"));
        Assert.Equal(11, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[10]); // true

        Assert.True(await forth.EvalAsync("-1.0d F0>"));
        Assert.Equal(12, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[11]); // false

        Assert.True(await forth.EvalAsync("0.0d F0>"));
        Assert.Equal(13, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[12]); // false

        // F0<=
        Assert.True(await forth.EvalAsync("-1.0d F0<="));
        Assert.Equal(14, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[13]); // true

        Assert.True(await forth.EvalAsync("1.0d F0<="));
        Assert.Equal(15, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[14]); // false

        Assert.True(await forth.EvalAsync("0.0d F0<="));
        Assert.Equal(16, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[15]); // true

        // F0>=
        Assert.True(await forth.EvalAsync("1.0d F0>="));
        Assert.Equal(17, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[16]); // true

        Assert.True(await forth.EvalAsync("-1.0d F0>="));
        Assert.Equal(18, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[17]); // false

        Assert.True(await forth.EvalAsync("0.0d F0>="));
        Assert.Equal(19, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[18]); // true
    }

    [Fact]
    public async Task FToS_ConvertsFloatToInt()
    {
        var forth = new ForthInterpreter();
        // Positive float
        Assert.True(await forth.EvalAsync("3.7d F>S"));
        Assert.Single(forth.Stack);
        Assert.Equal(3L, (long)forth.Stack[0]);

        // Negative float
        Assert.True(await forth.EvalAsync("-2.9d F>S"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-2L, (long)forth.Stack[1]);

        // Zero
        Assert.True(await forth.EvalAsync("0.0d F>S"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[2]);
    }

    [Fact]
    public async Task SToF_ConvertsIntToFloat()
    {
        var forth = new ForthInterpreter();
        // Positive integer
        Assert.True(await forth.EvalAsync("42 S>F"));
        Assert.Single(forth.Stack);
        Assert.Equal(42.0, (double)forth.Stack[0], 10);

        // Negative integer
        Assert.True(await forth.EvalAsync("-7 S>F"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-7.0, (double)forth.Stack[1], 10);

        // Zero
        Assert.True(await forth.EvalAsync("0 S>F"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(0.0, (double)forth.Stack[2], 10);
    }

    [Fact]
    public async Task FABS_AbsoluteValue()
    {
        var forth = new ForthInterpreter();
        // Positive float
        Assert.True(await forth.EvalAsync("3.5d FABS"));
        Assert.Single(forth.Stack);
        Assert.Equal(3.5, (double)forth.Stack[0], 10);

        // Negative float
        Assert.True(await forth.EvalAsync("-2.5d FABS"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(2.5, (double)forth.Stack[1], 10);

        // Zero
        Assert.True(await forth.EvalAsync("0.0d FABS"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(0.0, (double)forth.Stack[2], 10);
    }

    [Fact]
    public async Task FLOOR_Flooring()
    {
        var forth = new ForthInterpreter();
        // Positive float
        Assert.True(await forth.EvalAsync("3.7d FLOOR"));
        Assert.Single(forth.Stack);
        Assert.Equal(3L, (long)forth.Stack[0]);

        // Negative float
        Assert.True(await forth.EvalAsync("-2.1d FLOOR"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-3L, (long)forth.Stack[1]);

        // Integer
        Assert.True(await forth.EvalAsync("5.0d FLOOR"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(5L, (long)forth.Stack[2]);
    }

    [Fact]
    public async Task FROUND_Rounding()
    {
        var forth = new ForthInterpreter();
        // Positive float
        Assert.True(await forth.EvalAsync("3.7d FROUND"));
        Assert.Single(forth.Stack);
        Assert.Equal(4L, (long)forth.Stack[0]);

        // Negative float
        Assert.True(await forth.EvalAsync("-2.3d FROUND"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-2L, (long)forth.Stack[1]);

        // Integer
        Assert.True(await forth.EvalAsync("5.0d FROUND"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(5L, (long)forth.Stack[2]);

        // Tie to even
        Assert.True(await forth.EvalAsync("2.5d FROUND"));
        Assert.Equal(4, forth.Stack.Count);
        Assert.Equal(2L, (long)forth.Stack[3]);
    }

    [Fact]
    public async Task FSIN_Sine()
    {
        var forth = new ForthInterpreter();
        // sin(0) = 0
        Assert.True(await forth.EvalAsync("0.0d FSIN"));
        Assert.Single(forth.Stack);
        Assert.Equal(0.0, (double)forth.Stack[0], 10);

        // sin(pi/2) ? 1
        Assert.True(await forth.EvalAsync("1.5707963267948966d FSIN"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(1.0, (double)forth.Stack[1], 5);

        // sin(pi) ? 0
        Assert.True(await forth.EvalAsync("3.141592653589793d FSIN"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(0.0, (double)forth.Stack[2], 10);
    }

    [Fact]
    public async Task FTAN_Tangent()
    {
        var forth = new ForthInterpreter();
        // tan(0) = 0
        Assert.True(await forth.EvalAsync("0.0d FTAN"));
        Assert.Single(forth.Stack);
        Assert.Equal(0.0, (double)forth.Stack[0], 10);

        // tan(pi/4) ? 1
        Assert.True(await forth.EvalAsync("0.7853981633974483d FTAN"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(1.0, (double)forth.Stack[1], 5);
    }

    [Fact]
    public async Task FCOS_Cosine()
    {
        var forth = new ForthInterpreter();
        // cos(0) = 1
        Assert.True(await forth.EvalAsync("0.0d FCOS"));
        Assert.Single(forth.Stack);
        Assert.Equal(1.0, (double)forth.Stack[0], 10);

        // cos(pi) ? -1
        Assert.True(await forth.EvalAsync("3.141592653589793d FCOS"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-1.0, (double)forth.Stack[1], 5);
    }

    [Fact]
    public async Task FEXP_Exponential()
    {
        var forth = new ForthInterpreter();
        // exp(0) = 1
        Assert.True(await forth.EvalAsync("0.0d FEXP"));
        Assert.Single(forth.Stack);
        Assert.Equal(1.0, (double)forth.Stack[0], 10);

        // exp(1) ? e ? 2.718
        Assert.True(await forth.EvalAsync("1.0d FEXP"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(2.718281828459045, (double)forth.Stack[1], 5);
    }

    [Fact]
    public async Task FLOG_Logarithm()
    {
        var forth = new ForthInterpreter();
        // log(1) = 0
        Assert.True(await forth.EvalAsync("1.0d FLOG"));
        Assert.Single(forth.Stack);
        Assert.Equal(0.0, (double)forth.Stack[0], 10);

        // log(e) = 1
        Assert.True(await forth.EvalAsync("2.718281828459045d FLOG"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(1.0, (double)forth.Stack[1], 5);
    }

    [Fact]
    public async Task FACOS_Arccosine()
    {
        var forth = new ForthInterpreter();
        // acos(1) = 0
        Assert.True(await forth.EvalAsync("1.0d FACOS"));
        Assert.Single(forth.Stack);
        Assert.Equal(0.0, (double)forth.Stack[0], 10);

        // acos(0) = pi/2
        Assert.True(await forth.EvalAsync("0.0d FACOS"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(1.5707963267948966, (double)forth.Stack[1], 5);
    }

    [Fact]
    public async Task FASIN_Arcsine()
    {
        var forth = new ForthInterpreter();
        // asin(0) = 0
        Assert.True(await forth.EvalAsync("0.0d FASIN"));
        Assert.Single(forth.Stack);
        Assert.Equal(0.0, (double)forth.Stack[0], 10);

        // asin(1) = pi/2
        Assert.True(await forth.EvalAsync("1.0d FASIN"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(1.5707963267948966, (double)forth.Stack[1], 5);
    }

    [Fact]
    public async Task FAtan2_ArcTangent()
    {
        // atan2(1, 1) = pi/4
        var forth1 = new ForthInterpreter();
        Assert.True(await forth1.EvalAsync("1.0d 1.0d FATAN2"));
        Assert.Single(forth1.Stack);
        Assert.Equal(0.7853981633974483, (double)forth1.Stack[0], 10);

        // atan2(0, 1) = 0
        var forth2 = new ForthInterpreter();
        Assert.True(await forth2.EvalAsync("0.0d 1.0d FATAN2"));
        Assert.Single(forth2.Stack);
        Assert.Equal(0.0, (double)forth2.Stack[0], 10);
    }

    [Fact]
    public async Task FTilde_ApproximateEquality()
    {
        var forth = new ForthInterpreter();
        // 1.0 1.0001 0.01 F~ should be true (|1.0 - 1.0001| = 0.0001 < 0.01)
        Assert.True(await forth.EvalAsync("1.0d 1.0001d 0.01d F~"));
        Assert.Single(forth.Stack);
        Assert.Equal(-1L, (long)forth.Stack[0]); // true

        // 1.0 1.1 0.05 F~ should be false (|1.0 - 1.1| = 0.1 > 0.05)
        Assert.True(await forth.EvalAsync("1.0d 1.1d 0.05d F~"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[1]); // false

        // Close equality
        Assert.True(await forth.EvalAsync("2.5d 2.5001d 0.01d F~"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[2]); // true
    }

    [Fact]
    public async Task FMin_ReturnsMinimum()
    {
        var forth = new ForthInterpreter();
        // 3.5 2.1 FMIN should return 2.1
        Assert.True(await forth.EvalAsync("3.5d 2.1d FMIN"));
        Assert.Single(forth.Stack);
        Assert.Equal(2.1, (double)forth.Stack[0]);

        // 1.0 1.0 FMIN should return 1.0
        Assert.True(await forth.EvalAsync("1.0d 1.0d FMIN"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(1.0, (double)forth.Stack[1]);
    }

    [Fact]
    public async Task FMax_ReturnsMaximum()
    {
        var forth = new ForthInterpreter();
        // 3.5 2.1 FMAX should return 3.5
        Assert.True(await forth.EvalAsync("3.5d 2.1d FMAX"));
        Assert.Single(forth.Stack);
        Assert.Equal(3.5, (double)forth.Stack[0]);

        // 1.0 1.0 FMAX should return 1.0
        Assert.True(await forth.EvalAsync("1.0d 1.0d FMAX"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(1.0, (double)forth.Stack[1]);
    }

    [Fact]
    public async Task FSQRT_SquareRoot()
    {
        var forth = new ForthInterpreter();
        // sqrt(4) = 2
        Assert.True(await forth.EvalAsync("4.0d FSQRT"));
        Assert.Single(forth.Stack);
        Assert.Equal(2.0, (double)forth.Stack[0], 10);

        // sqrt(9) = 3
        Assert.True(await forth.EvalAsync("9.0d FSQRT"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(3.0, (double)forth.Stack[1], 10);

        // sqrt(0) = 0
        Assert.True(await forth.EvalAsync("0.0d FSQRT"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(0.0, (double)forth.Stack[2], 10);
    }

    [Fact]
    public async Task FTRUNC_TruncateTowardsZero()
    {
        var forth = new ForthInterpreter();
        // Positive float
        Assert.True(await forth.EvalAsync("3.7d FTRUNC"));
        Assert.Single(forth.Stack);
        Assert.Equal(3L, (long)forth.Stack[0]);

        // Negative float
        Assert.True(await forth.EvalAsync("-2.9d FTRUNC"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-2L, (long)forth.Stack[1]);

        // Integer
        Assert.True(await forth.EvalAsync("5.0d FTRUNC"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(5L, (long)forth.Stack[2]);
    }

    [Fact]
    public async Task FloatingPoint_StackOperations()
    {
        var forth = new ForthInterpreter();
        // FSWAP
        Assert.True(await forth.EvalAsync("1.0d 2.0d FSWAP"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(2.0, (double)forth.Stack[0]);
        Assert.Equal(1.0, (double)forth.Stack[1]);

        // FROT
        Assert.True(await forth.EvalAsync("3.0d FROT"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(1.0, (double)forth.Stack[0]);
        Assert.Equal(3.0, (double)forth.Stack[1]);
        Assert.Equal(2.0, (double)forth.Stack[2]);
    }

    [Fact]
    public async Task FPow_Exponentiation()
    {
        var forth = new ForthInterpreter();
        // 2^3 = 8
        Assert.True(await forth.EvalAsync("2.0d 3.0d F**"));
        Assert.Single(forth.Stack);
        Assert.Equal(8.0, (double)forth.Stack[0], 10);

        // 4^0.5 = 2
        Assert.True(await forth.EvalAsync("4.0d 0.5d F**"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(2.0, (double)forth.Stack[1], 10);

        // 1^anything = 1
        Assert.True(await forth.EvalAsync("1.0d 10.0d F**"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(1.0, (double)forth.Stack[2], 10);
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
