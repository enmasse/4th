using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class DoubleNumberTests
{
    [Fact]
    public async Task DLess_EqualDoubles_ReturnsFalse()
    {
        var f = new ForthInterpreter();
        // 5 0 5 0 D< should be false
        Assert.True(await f.EvalAsync("5 0 5 0 D<"));
        Assert.Single(f.Stack);
        Assert.Equal(0L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task DLess_FirstLessThanSecond_ReturnsTrue()
    {
        var f = new ForthInterpreter();
        // 4 0 5 0 D< should be true
        Assert.True(await f.EvalAsync("4 0 5 0 D<"));
        Assert.Single(f.Stack);
        Assert.Equal(-1L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task DLess_FirstGreaterThanSecond_ReturnsFalse()
    {
        var f = new ForthInterpreter();
        // 6 0 5 0 D< should be false
        Assert.True(await f.EvalAsync("6 0 5 0 D<"));
        Assert.Single(f.Stack);
        Assert.Equal(0L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task DLess_NegativeDoubles()
    {
        var f = new ForthInterpreter();
        // -5 -1 -4 -1 D< should be true (-5 < -4)
        Assert.True(await f.EvalAsync("-5 -1 -4 -1 D<"));
        Assert.Single(f.Stack);
        Assert.Equal(-1L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task DLess_MixedSigns()
    {
        var f = new ForthInterpreter();
        // -1 -1 1 0 D< should be true (negative < positive)
        Assert.True(await f.EvalAsync("-1 -1 1 0 D<"));
        Assert.Single(f.Stack);
        Assert.Equal(-1L, (long)f.Stack[0]);

        var f2 = new ForthInterpreter();
        // 1 0 -1 -1 D< should be false (positive > negative)
        Assert.True(await f2.EvalAsync("1 0 -1 -1 D<"));
        Assert.Single(f2.Stack);
        Assert.Equal(0L, (long)f2.Stack[0]);
    }

    [Fact]
    public async Task DLess_LargeNumbers()
    {
        var f = new ForthInterpreter();
        // Test with large numbers, assuming long max
        // long.MaxValue 0 long.MaxValue-1 0 D< should be false
        Assert.True(await f.EvalAsync($"{long.MaxValue} 0 {long.MaxValue - 1} 0 D<"));
        Assert.Single(f.Stack);
        Assert.Equal(0L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task DEqual_EqualDoubles_ReturnsTrue()
    {
        var f = new ForthInterpreter();
        // 5 0 5 0 D= should be true
        Assert.True(await f.EvalAsync("5 0 5 0 D="));
        Assert.Single(f.Stack);
        Assert.Equal(-1L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task DEqual_UnequalDoubles_ReturnsFalse()
    {
        var f = new ForthInterpreter();
        // 4 0 5 0 D= should be false
        Assert.True(await f.EvalAsync("4 0 5 0 D="));
        Assert.Single(f.Stack);
        Assert.Equal(0L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task DEqual_NegativeDoubles_Equal()
    {
        var f = new ForthInterpreter();
        // -5 -1 -5 -1 D= should be true
        Assert.True(await f.EvalAsync("-5 -1 -5 -1 D="));
        Assert.Single(f.Stack);
        Assert.Equal(-1L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task DEqual_NegativeDoubles_Unequal()
    {
        var f = new ForthInterpreter();
        // -5 -1 -4 -1 D= should be false
        Assert.True(await f.EvalAsync("-5 -1 -4 -1 D="));
        Assert.Single(f.Stack);
        Assert.Equal(0L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task DEqual_MixedSigns_Unequal()
    {
        var f = new ForthInterpreter();
        // -1 -1 1 0 D= should be false
        Assert.True(await f.EvalAsync("-1 -1 1 0 D="));
        Assert.Single(f.Stack);
        Assert.Equal(0L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task DEqual_LargeNumbers_Equal()
    {
        var f = new ForthInterpreter();
        // Test with large numbers
        Assert.True(await f.EvalAsync($"{long.MaxValue} 0 {long.MaxValue} 0 D="));
        Assert.Single(f.Stack);
        Assert.Equal(-1L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task DEqual_LargeNumbers_Unequal()
    {
        var f = new ForthInterpreter();
        // long.MaxValue 0 long.MaxValue-1 0 D= should be false
        Assert.True(await f.EvalAsync($"{long.MaxValue} 0 {long.MaxValue - 1} 0 D="));
        Assert.Single(f.Stack);
        Assert.Equal(0L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task DToS_PositiveDouble()
    {
        var f = new ForthInterpreter();
        // 5 10 D>S should return 10
        Assert.True(await f.EvalAsync("5 10 D>S"));
        Assert.Single(f.Stack);
        Assert.Equal(10L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task DToS_NegativeDouble()
    {
        var f = new ForthInterpreter();
        // -1 -5 D>S should return -5
        Assert.True(await f.EvalAsync("-1 -5 D>S"));
        Assert.Single(f.Stack);
        Assert.Equal(-5L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task DToS_ZeroHigh()
    {
        var f = new ForthInterpreter();
        // 123 0 D>S should return 0
        Assert.True(await f.EvalAsync("123 0 D>S"));
        Assert.Single(f.Stack);
        Assert.Equal(0L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task DToS_LargeNumbers()
    {
        var f = new ForthInterpreter();
        // Test with large high cell
        Assert.True(await f.EvalAsync($"{long.MaxValue} {long.MaxValue - 1} D>S"));
        Assert.Single(f.Stack);
        Assert.Equal(long.MaxValue - 1, (long)f.Stack[0]);
    }

    [Fact]
    public async Task DTwoStar_PositiveDouble()
    {
        var f = new ForthInterpreter();
        // 5 0 D2* should return 10 0
        Assert.True(await f.EvalAsync("5 0 D2*"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(10L, (long)f.Stack[0]);
        Assert.Equal(0L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DTwoStar_CarryOver()
    {
        var f = new ForthInterpreter();
        // 0 1 D2* should return 0 2
        Assert.True(await f.EvalAsync("0 1 D2*"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(0L, (long)f.Stack[0]);
        Assert.Equal(2L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DTwoStar_NegativeDouble()
    {
        var f = new ForthInterpreter();
        // -1 -1 D2* should return -2 -1
        Assert.True(await f.EvalAsync("-1 -1 D2*"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(-2L, (long)f.Stack[0])
        ;Assert.Equal(-1L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DTwoStar_LargeNumbers()
    {
        var f = new ForthInterpreter();
        // Test with large numbers, e.g., long.MaxValue 0 D2* should overflow to low= -2, high=0 (since 2*MaxValue = 2^64 - 2, low=-2, high=0)
        Assert.True(await f.EvalAsync($"{long.MaxValue} 0 D2*"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(-2L, (long)f.Stack[0]); // low
        Assert.Equal(0L, (long)f.Stack[1]); // high
    }

    [Fact]
    public async Task DTwoSlash_PositiveEven()
    {
        var f = new ForthInterpreter();
        // 2 0 D2/ should return 1 0
        Assert.True(await f.EvalAsync("2 0 D2/"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(1L, (long)f.Stack[0]);
        Assert.Equal(0L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DTwoSlash_PositiveOdd()
    {
        var f = new ForthInterpreter();
        // 3 0 D2/ should return 1 0 (truncate toward zero? No, shift right)
        Assert.True(await f.EvalAsync("3 0 D2/"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(1L, (long)f.Stack[0]);
        Assert.Equal(0L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DTwoSlash_CarryOver()
    {
        var f = new ForthInterpreter();
        // 0 1 D2/ should return long.MinValue 0 (2^63 as signed)
        Assert.True(await f.EvalAsync("0 1 D2/"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(long.MinValue, (long)f.Stack[0]); // 2^63
        Assert.Equal(0L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DTwoSlash_NegativeEven()
    {
        var f = new ForthInterpreter();
        // -2 0 D2/ should return -1 0
        Assert.True(await f.EvalAsync("-2 0 D2/"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(-1L, (long)f.Stack[0]);
        Assert.Equal(0L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DTwoSlash_NegativeOdd()
    {
        var f = new ForthInterpreter();
        // -1 -1 D2/ should return -1 -1 (arithmetic shift)
        Assert.True(await f.EvalAsync("-1 -1 D2/"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(-1L, (long)f.Stack[0]);
        Assert.Equal(-1L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DABS_PositiveDouble()
    {
        var f = new ForthInterpreter();
        // 5 0 DABS should return 5 0
        Assert.True(await f.EvalAsync("5 0 DABS"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(5L, (long)f.Stack[0]);
        Assert.Equal(0L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DABS_NegativeDouble()
    {
        var f = new ForthInterpreter();
        // -5 0 DABS should return 5 0
        Assert.True(await f.EvalAsync("-5 0 DABS"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(5L, (long)f.Stack[0]);
        Assert.Equal(0L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DABS_Zero()
    {
        var f = new ForthInterpreter();
        // 0 0 DABS should return 0 0
        Assert.True(await f.EvalAsync("0 0 DABS"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(0L, (long)f.Stack[0]);
        Assert.Equal(0L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DABS_LargeNegative()
    {
        var f = new ForthInterpreter();
        // -1 -1 DABS should return 1 1 (2^64 +1)
        Assert.True(await f.EvalAsync("-1 -1 DABS"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(1L, (long)f.Stack[0]);
        Assert.Equal(1L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DABS_MinValue()
    {
        var f = new ForthInterpreter();
        // -1 0 DABS should return 1 0
        Assert.True(await f.EvalAsync("-1 0 DABS"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(1L, (long)f.Stack[0]);
        Assert.Equal(0L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DNEGATE_PositiveDouble()
    {
        var f = new ForthInterpreter();
        // 5 0 DNEGATE should return -5 -1 (since -5 sign-extended)
        Assert.True(await f.EvalAsync("5 0 DNEGATE"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(-5L, (long)f.Stack[0]);
        Assert.Equal(-1L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DNEGATE_NegativeDouble()
    {
        var f = new ForthInterpreter();
        // -5 -1 DNEGATE should return 5 1
        Assert.True(await f.EvalAsync("-5 -1 DNEGATE"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(5L, (long)f.Stack[0]);
        Assert.Equal(1L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DNEGATE_Zero()
    {
        var f = new ForthInterpreter();
        // 0 0 DNEGATE should return 0 0
        Assert.True(await f.EvalAsync("0 0 DNEGATE"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(0L, (long)f.Stack[0]);
        Assert.Equal(0L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DNEGATE_LargePositive()
    {
        var f = new ForthInterpreter();
        // 0 1 DNEGATE should return 0 -1 (negate 2^64)
        Assert.True(await f.EvalAsync("0 1 DNEGATE"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(0L, (long)f.Stack[0]);
        Assert.Equal(-1L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DNEGATE_LargeNegative()
    {
        var f = new ForthInterpreter();
        // 0 -1 DNEGATE should return 0 1
        Assert.True(await f.EvalAsync("0 -1 DNEGATE"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(0L, (long)f.Stack[0]);
        Assert.Equal(1L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DULess_EqualUnsignedDoubles_ReturnsFalse()
    {
        var f = new ForthInterpreter();
        // 5 0 5 0 DU< should be false
        Assert.True(await f.EvalAsync("5 0 5 0 DU<"));
        Assert.Single(f.Stack);
        Assert.Equal(0L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task DULess_FirstLessThanSecond_ReturnsTrue()
    {
        var f = new ForthInterpreter();
        // 4 0 5 0 DU< should be true
        Assert.True(await f.EvalAsync("4 0 5 0 DU<"));
        Assert.Single(f.Stack);
        Assert.Equal(-1L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task DULess_FirstGreaterThanSecond_ReturnsFalse()
    {
        var f = new ForthInterpreter();
        // 6 0 5 0 DU< should be false
        Assert.True(await f.EvalAsync("6 0 5 0 DU<"));
        Assert.Single(f.Stack);
        Assert.Equal(0L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task DULess_NegativeVsPositive()
    {
        var f = new ForthInterpreter();
        // -1 -1 0 0 DU< should be false (large unsigned vs small)
        Assert.True(await f.EvalAsync("-1 -1 0 0 DU<"));
        Assert.Single(f.Stack);
        Assert.Equal(0L, (long)f.Stack[0]);

        var f2 = new ForthInterpreter();
        // 0 0 -1 -1 DU< should be true (small vs large)
        Assert.True(await f2.EvalAsync("0 0 -1 -1 DU<"));
        Assert.Single(f2.Stack);
        Assert.Equal(-1L, (long)f2.Stack[0]);
    }

    [Fact]
    public async Task DULess_LargeNumbers()
    {
        var f = new ForthInterpreter();
        // long.MaxValue 0 long.MaxValue-1 0 DU< should be false
        Assert.True(await f.EvalAsync($"{long.MaxValue} 0 {long.MaxValue - 1} 0 DU<"));
        Assert.Single(f.Stack);
        Assert.Equal(0L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task DMAX_FirstLarger()
    {
        var f = new ForthInterpreter();
        // 6 0 5 0 DMAX should return 6 0
        Assert.True(await f.EvalAsync("6 0 5 0 DMAX"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(6L, (long)f.Stack[0]);
        Assert.Equal(0L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DMAX_SecondLarger()
    {
        var f = new ForthInterpreter();
        // 4 0 5 0 DMAX should return 5 0
        Assert.True(await f.EvalAsync("4 0 5 0 DMAX"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(5L, (long)f.Stack[0]);
        Assert.Equal(0L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DMAX_Equal()
    {
        var f = new ForthInterpreter();
        // 5 0 5 0 DMAX should return 5 0
        Assert.True(await f.EvalAsync("5 0 5 0 DMAX"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(5L, (long)f.Stack[0]);
        Assert.Equal(0L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DMAX_NegativeNumbers()
    {
        var f = new ForthInterpreter();
        // -4 -1 -5 -1 DMAX should return -4 -1 (-4 > -5)
        Assert.True(await f.EvalAsync("-4 -1 -5 -1 DMAX"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(-4L, (long)f.Stack[0]);
        Assert.Equal(-1L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DMAX_LargeNumbers()
    {
        var f = new ForthInterpreter();
        // long.MaxValue 0 long.MaxValue-1 0 DMAX should return long.MaxValue 0
        Assert.True(await f.EvalAsync($"{long.MaxValue} 0 {long.MaxValue - 1} 0 DMAX"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(long.MaxValue, (long)f.Stack[0]);
        Assert.Equal(0L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DMIN_FirstSmaller()
    {
        var f = new ForthInterpreter();
        // 4 0 5 0 DMIN should return 4 0
        Assert.True(await f.EvalAsync("4 0 5 0 DMIN"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(4L, (long)f.Stack[0]);
        Assert.Equal(0L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DMIN_SecondSmaller()
    {
        var f = new ForthInterpreter();
        // 6 0 5 0 DMIN should return 5 0
        Assert.True(await f.EvalAsync("6 0 5 0 DMIN"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(5L, (long)f.Stack[0]);
        Assert.Equal(0L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DMIN_Equal()
    {
        var f = new ForthInterpreter();
        // 5 0 5 0 DMIN should return 5 0
        Assert.True(await f.EvalAsync("5 0 5 0 DMIN"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(5L, (long)f.Stack[0]);
        Assert.Equal(0L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DMIN_NegativeNumbers()
    {
        var f = new ForthInterpreter();
        // -4 -1 -5 -1 DMIN should return -5 -1 (-5 < -4)
        Assert.True(await f.EvalAsync("-4 -1 -5 -1 DMIN"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(-5L, (long)f.Stack[0]);
        Assert.Equal(-1L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DMIN_LargeNumbers()
    {
        var f = new ForthInterpreter();
        // long.MaxValue 0 long.MaxValue-1 0 DMIN should return long.MaxValue-1 0
        Assert.True(await f.EvalAsync($"{long.MaxValue} 0 {long.MaxValue - 1} 0 DMIN"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(long.MaxValue - 1, (long)f.Stack[0]);
        Assert.Equal(0L, (long)f.Stack[1]);
    }

    [Fact]
    public async Task DDot_PrintsDouble()
    {
        var io = new TestIO();
        var f = new ForthInterpreter(io);
        // 123 0 D. should print 123
        Assert.True(await f.EvalAsync("123 0 D."));
        Assert.Single(io.Outputs);
        Assert.Equal("123", io.Outputs[0]);
    }

    [Fact]
    public async Task DDot_PrintsNegativeDouble()
    {
        var io = new TestIO();
        var f = new ForthInterpreter(io);
        // -123 -1 D. should print -123
        Assert.True(await f.EvalAsync("-123 -1 D."));
        Assert.Single(io.Outputs);
        Assert.Equal("-123", io.Outputs[0]);
    }

    [Fact]
    public async Task DDot_PrintsLargeDouble()
    {
        var io = new TestIO();
        var f = new ForthInterpreter(io);
        // 0 1 D. should print 18446744073709551616 (2^64)
        Assert.True(await f.EvalAsync("0 1 D."));
        Assert.Single(io.Outputs);
        Assert.Equal("18446744073709551616", io.Outputs[0]);
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