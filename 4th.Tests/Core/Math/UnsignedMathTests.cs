using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core.Math;

public class UnsignedMathTests
{
    [Fact]
    public async Task ZeroGreater_Basics()
    {
        var f = new ForthInterpreter();
        Assert.True(await f.EvalAsync("-1 0>"));
        Assert.Equal(0L, (long)f.Stack[^1]);
        Assert.True(await f.EvalAsync("0 0>"));
        Assert.Equal(0L, (long)f.Stack[^1]);
        Assert.True(await f.EvalAsync("1 0>"));
        Assert.Equal(-1L, (long)f.Stack[^1]);
    }

    [Fact]
    public async Task ULess_Basics()
    {
        var f = new ForthInterpreter();
        Assert.True(await f.EvalAsync("0 -1 U<"));
        Assert.Equal(-1L, (long)f.Stack[^1]);
        Assert.True(await f.EvalAsync("-1 0 U<"));
        Assert.Equal(0L, (long)f.Stack[^1]);
        Assert.True(await f.EvalAsync("-2 -1 U<"));
        Assert.Equal(-1L, (long)f.Stack[^1]);
    }

    [Fact]
    public async Task UMStar_Basics()
    {
        var f = new ForthInterpreter();
        Assert.True(await f.EvalAsync("3 4 UM*"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(12L, (long)f.Stack[0]);
        Assert.Equal(0L, (long)f.Stack[1]);

        var f2 = new ForthInterpreter();
        Assert.True(await f2.EvalAsync("4294967296 4294967296 UM*"));
        Assert.Equal(2, f2.Stack.Count);
        Assert.Equal(0L, (long)f2.Stack[0]);
        Assert.Equal(1L, (long)f2.Stack[1]);
    }

    [Fact]
    public async Task UMSlashMod_Basics()
    {
        var f = new ForthInterpreter();
        Assert.True(await f.EvalAsync("7 0 3 UM/MOD"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(1L, (long)f.Stack[0]);
        Assert.Equal(2L, (long)f.Stack[1]);

        var f2 = new ForthInterpreter();
        Assert.True(await f2.EvalAsync("-1 0 2 UM/MOD"));
        Assert.Equal(2, f2.Stack.Count);
        Assert.Equal(1L, (long)f2.Stack[0]);
        Assert.Equal(9223372036854775807L, (long)f2.Stack[1]);
    }
}
