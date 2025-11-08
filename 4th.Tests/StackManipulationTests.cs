using Forth;
using Xunit;
using System.Linq;

namespace Forth.Tests;

public class StackManipulationTests
{
    private static IForthInterpreter New() => new ForthInterpreter();
    private static long[] Longs(IForthInterpreter f) => f.Stack.Select(o => o is long l ? l : o is int i ? (long)i : 0L).ToArray();

    [Fact]
    public async Task Dup_DuplicatesTop()
    {
        var forth = New();
        Assert.True(await forth.InterpretAsync("42 DUP"));
        Assert.Equal(new long[] { 42, 42 }, Longs(forth));
    }

    [Fact]
    public async Task Drop_RemovesTop()
    {
        var forth = New();
        Assert.True(await forth.InterpretAsync("1 2 DROP"));
        Assert.Equal(new long[] { 1 }, Longs(forth));
    }

    [Fact]
    public async Task Swap_ExchangesTopTwo()
    {
        var forth = New();
        Assert.True(await forth.InterpretAsync("1 2 SWAP"));
        Assert.Equal(new long[] { 2, 1 }, Longs(forth));
    }

    [Fact]
    public async Task Over_CopiesSecondToTop()
    {
        var forth = New();
        Assert.True(await forth.InterpretAsync("1 2 OVER"));
        Assert.Equal(new long[] { 1, 2, 1 }, Longs(forth));
    }
}
