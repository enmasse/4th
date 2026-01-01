using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class DoubleCellArithmeticTests
{
    [Fact]
    public async Task DPlus_NoCarry()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1 0 2 0 D+"));
        Assert.Equal(new long[] { 3, 0 }, forth.Stack.Select(o => (long)o).ToArray());
    }

    [Fact]
    public async Task DPlus_WithCarry()
    {
        var forth = new ForthInterpreter();
        // -1 as low part (all bits set) + 1 should wrap to 0 and produce a carry into high
        Assert.True(await forth.EvalAsync("-1 0 1 0 D+"));
        Assert.Equal(new long[] { 0, 1 }, forth.Stack.Select(o => (long)o).ToArray());
    }

    [Fact]
    public async Task DMinus_NoBorrow()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("5 0 3 0 D-"));
        Assert.Equal(new long[] { 2, 0 }, forth.Stack.Select(o => (long)o).ToArray());
    }

    [Fact]
    public async Task DMinus_WithBorrow()
    {
        var forth = new ForthInterpreter();
        // (0,1) - (1,0) -> low underflows producing -1 (all bits set) and high becomes 0
        Assert.True(await forth.EvalAsync("0 1 1 0 D-"));
        Assert.Equal(new long[] { -1, 0 }, forth.Stack.Select(o => (long)o).ToArray());
    }

    [Fact]
    public async Task MStar_Small()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("3 4 M*"));
        Assert.Equal(new long[] { 12, 0 }, forth.Stack.Select(o => (long)o).ToArray());
    }

    [Fact]
    public async Task MStar_Large128()
    {
        var forth = new ForthInterpreter();
        // Multiply 2^32 by 2^32 => 2^64 -> low 0, high 1
        Assert.True(await forth.EvalAsync("4294967296 4294967296 M*"));
        Assert.Equal(new long[] { 0, 1 }, forth.Stack.Select(o => (long)o).ToArray());
    }
}
