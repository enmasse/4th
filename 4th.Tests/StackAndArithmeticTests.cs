using Forth;
using Xunit;
using System.Linq;

namespace Forth.Tests;

public class StackAndArithmeticTests
{
    private static IForthInterpreter New() => new ForthInterpreter();
    private static long[] Longs(IForthInterpreter f) => f.Stack.Select(o => o is long l ? l : o is int i ? (long)i : 0L).ToArray();

    [Fact]
    public void PushNumbersOntoStack()
    {
        var forth = New();
        Assert.True(forth.Interpret("1 2 3"));
        Assert.Equal(new long[] { 1, 2, 3 }, Longs(forth));
    }

    [Fact]
    public void Addition()
    {
        var forth = New();
        Assert.True(forth.Interpret("1 2 +"));
        Assert.Equal(new long[] { 3 }, Longs(forth));
    }

    [Fact]
    public void Subtraction()
    {
        var forth = New();
        Assert.True(forth.Interpret("5 2 -"));
        Assert.Equal(new long[] { 3 }, Longs(forth));
    }

    [Fact]
    public void Multiplication()
    {
        var forth = New();
        Assert.True(forth.Interpret("4 3 *"));
        Assert.Equal(new long[] { 12 }, Longs(forth));
    }

    [Fact]
    public void Division_TruncatesTowardZero()
    {
        var forth = New();
        Assert.True(forth.Interpret("7 2 /"));
        Assert.Equal(new long[] { 3 }, Longs(forth));
    }
}
