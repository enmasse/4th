using Forth;
using Xunit;

namespace Forth.Tests;

public class StackAndArithmeticTests
{
    private static IForthInterpreter New() => new ForthInterpreter();

    [Fact]
    public void PushNumbersOntoStack()
    {
        var forth = New();
        Assert.True(forth.Interpret("1 2 3"));
        Assert.Equal(new long[] { 1, 2, 3 }, forth.Stack);
    }

    [Fact]
    public void Addition()
    {
        var forth = New();
        Assert.True(forth.Interpret("1 2 +"));
        Assert.Equal(new long[] { 3 }, forth.Stack);
    }

    [Fact]
    public void Subtraction()
    {
        var forth = New();
        Assert.True(forth.Interpret("5 2 -"));
        Assert.Equal(new long[] { 3 }, forth.Stack);
    }

    [Fact]
    public void Multiplication()
    {
        var forth = New();
        Assert.True(forth.Interpret("4 3 *"));
        Assert.Equal(new long[] { 12 }, forth.Stack);
    }

    [Fact]
    public void Division_TruncatesTowardZero()
    {
        var forth = New();
        Assert.True(forth.Interpret("7 2 /"));
        Assert.Equal(new long[] { 3 }, forth.Stack);
    }
}
