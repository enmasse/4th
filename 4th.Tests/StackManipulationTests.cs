using Forth;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Tests;

public class StackManipulationTests
{
    /// <summary>
    /// Creates a new interpreter instance for tests.
    /// </summary>
    private static IForthInterpreter New() => new ForthInterpreter();
    /// <summary>
    /// Projects stack objects to longs (non-numeric mapped to 0) for comparison convenience.
    /// </summary>
    private static long[] Longs(IForthInterpreter f) => f.Stack.Select(o => o is long l ? l : o is int i ? (long)i : 0L).ToArray();

    /// <summary>
    /// DUP should duplicate top of stack.
    /// </summary>
    [Fact]
    public async Task Dup_DuplicatesTop()
    {
        var forth = New();
        Assert.True(await forth.EvalAsync("42 DUP"));
        Assert.Equal(new long[] { 42, 42 }, Longs(forth));
    }

    /// <summary>
    /// DROP should remove top of stack.
    /// </summary>
    [Fact]
    public async Task Drop_RemovesTop()
    {
        var forth = New();
        Assert.True(await forth.EvalAsync("1 2 DROP"));
        Assert.Equal(new long[] { 1 }, Longs(forth));
    }

    /// <summary>
    /// SWAP should exchange top two stack items.
    /// </summary>
    [Fact]
    public async Task Swap_ExchangesTopTwo()
    {
        var forth = New();
        Assert.True(await forth.EvalAsync("1 2 SWAP"));
        Assert.Equal(new long[] { 2, 1 }, Longs(forth));
    }

    /// <summary>
    /// OVER should copy second item to top.
    /// </summary>
    [Fact]
    public async Task Over_CopiesSecondToTop()
    {
        var forth = New();
        Assert.True(await forth.EvalAsync("1 2 OVER"));
        Assert.Equal(new long[] { 1, 2, 1 }, Longs(forth));
    }
}
