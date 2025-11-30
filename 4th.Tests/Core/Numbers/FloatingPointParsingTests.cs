using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core.Numbers;

public class FloatingPointParsingTests
{
    [Fact]
    public async Task SimpleDecimal_ParsesAsFloat()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1.5"));
        Assert.Single(forth.Stack);
        Assert.IsType<double>(forth.Stack[0]);
        Assert.Equal(1.5, (double)forth.Stack[0]);
    }

    [Fact]
    public async Task NegativeDecimal_ParsesAsFloat()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("-3.14"));
        Assert.Single(forth.Stack);
        Assert.IsType<double>(forth.Stack[0]);
        Assert.Equal(-3.14, (double)forth.Stack[0]);
    }

    [Fact]
    public async Task DecimalWithLeadingZero_ParsesAsFloat()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("0.5"));
        Assert.Single(forth.Stack);
        Assert.IsType<double>(forth.Stack[0]);
        Assert.Equal(0.5, (double)forth.Stack[0]);
    }

    [Fact]
    public async Task DecimalWithTrailingZero_ParsesAsFloat()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("2.0"));
        Assert.Single(forth.Stack);
        Assert.IsType<double>(forth.Stack[0]);
        Assert.Equal(2.0, (double)forth.Stack[0]);
    }

    [Fact]
    public async Task ExponentNotation_StillWorks()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1.5e2"));
        Assert.Single(forth.Stack);
        Assert.IsType<double>(forth.Stack[0]);
        Assert.Equal(150.0, (double)forth.Stack[0]);
    }

    [Fact]
    public async Task ExponentWithoutDecimal_StillWorks()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("3e2"));
        Assert.Single(forth.Stack);
        Assert.IsType<double>(forth.Stack[0]);
        Assert.Equal(300.0, (double)forth.Stack[0]);
    }

    [Fact]
    public async Task DSuffix_StillWorks()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1.5d"));
        Assert.Single(forth.Stack);
        Assert.IsType<double>(forth.Stack[0]);
        Assert.Equal(1.5, (double)forth.Stack[0]);
    }

    [Fact]
    public async Task Integer_ParsesAsLong()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("42"));
        Assert.Single(forth.Stack);
        Assert.IsType<long>(forth.Stack[0]);
        Assert.Equal(42L, (long)forth.Stack[0]);
    }

    [Fact]
    public async Task MultipleFloats_ParseCorrectly()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1.5 2.5 3.5"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(1.5, (double)forth.Stack[0]);
        Assert.Equal(2.5, (double)forth.Stack[1]);
        Assert.Equal(3.5, (double)forth.Stack[2]);
    }

    [Fact]
    public async Task MixedIntAndFloat_ParseCorrectly()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("10 3.14 20"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(10L, (long)forth.Stack[0]);
        Assert.Equal(3.14, (double)forth.Stack[1]);
        Assert.Equal(20L, (long)forth.Stack[2]);
    }

    [Fact]
    public async Task ZeroPointZero_ParsesAsFloat()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("0.0"));
        Assert.Single(forth.Stack);
        Assert.IsType<double>(forth.Stack[0]);
        Assert.Equal(0.0, (double)forth.Stack[0]);
    }

    [Fact]
    public async Task LargeDecimal_ParsesCorrectly()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("123456.789"));
        Assert.Single(forth.Stack);
        Assert.IsType<double>(forth.Stack[0]);
        Assert.Equal(123456.789, (double)forth.Stack[0]);
    }

    [Fact]
    public async Task NaN_ParsesCorrectly()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("NaN"));
        Assert.Single(forth.Stack);
        Assert.IsType<double>(forth.Stack[0]);
        Assert.True(double.IsNaN((double)forth.Stack[0]));
    }

    [Fact]
    public async Task Infinity_ParsesCorrectly()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("Infinity"));
        Assert.Single(forth.Stack);
        Assert.IsType<double>(forth.Stack[0]);
        Assert.True(double.IsPositiveInfinity((double)forth.Stack[0]));
    }

    [Fact]
    public async Task NegativeInfinity_ParsesCorrectly()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("-Infinity"));
        Assert.Single(forth.Stack);
        Assert.IsType<double>(forth.Stack[0]);
        Assert.True(double.IsNegativeInfinity((double)forth.Stack[0]));
    }
}
