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
}