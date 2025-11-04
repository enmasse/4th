using Forth;
using Xunit;

namespace Forth.Tests;

public class StackManipulationTests
{
    private static IForthInterpreter New() => new ForthInterpreter();

    [Fact]
    public void Dup_DuplicatesTop()
    {
        var forth = New();
        Assert.True(forth.Interpret("42 DUP"));
        Assert.Equal(new long[] { 42, 42 }, forth.Stack);
    }

    [Fact]
    public void Drop_RemovesTop()
    {
        var forth = New();
        Assert.True(forth.Interpret("1 2 DROP"));
        Assert.Equal(new long[] { 1 }, forth.Stack);
    }

    [Fact]
    public void Swap_ExchangesTopTwo()
    {
        var forth = New();
        Assert.True(forth.Interpret("1 2 SWAP"));
        Assert.Equal(new long[] { 2, 1 }, forth.Stack);
    }

    [Fact]
    public void Over_CopiesSecondToTop()
    {
        var forth = New();
        Assert.True(forth.Interpret("1 2 OVER"));
        Assert.Equal(new long[] { 1, 2, 1 }, forth.Stack);
    }
}
