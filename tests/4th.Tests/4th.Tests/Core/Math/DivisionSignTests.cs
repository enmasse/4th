using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core.Math;

public class DivisionSignTests
{
    [Fact]
    public async Task SlashMod_NegativeOperands_TruncatesTowardZero()
    {
        var forth = new ForthInterpreter();
        // -7 3 -> quotient -2, remainder -1 (C# truncates toward zero)
        Assert.True(await forth.EvalAsync("-7 3 /MOD"));
        Assert.Equal(-2L, (long)forth.Stack[^1]); // quotient
        Assert.Equal(-1L, (long)forth.Stack[^2]); // remainder

        // 7 -3 -> quotient -2, remainder 1
        Assert.True(await forth.EvalAsync("7 -3 /MOD"));
        Assert.Equal(-2L, (long)forth.Stack[^1]);
        Assert.Equal(1L, (long)forth.Stack[^2]);

        // -7 -3 -> quotient 2, remainder -1
        Assert.True(await forth.EvalAsync("-7 -3 /MOD"));
        Assert.Equal(2L, (long)forth.Stack[^1]);
        Assert.Equal(-1L, (long)forth.Stack[^2]);
    }

    [Fact]
    public async Task Slash_And_MOD_Individual_NegativeCases()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("-7 3 /"));
        Assert.Equal(-2L, (long)forth.Stack[^1]);

        Assert.True(await forth.EvalAsync("-7 3 MOD"));
        Assert.Equal(-1L, (long)forth.Stack[^1]);
    }

    [Fact]
    public async Task StarSlashMOD_ViaPrelude_Works_OnPositives()
    {
        var forth = new ForthInterpreter();
        // prelude defines */MOD in terms of /MOD; ensure it produces expected result
        Assert.True(await forth.EvalAsync("10 7 3 */MOD"));
        Assert.Equal(23L, (long)forth.Stack[^1]); // quotient
        Assert.Equal(1L, (long)forth.Stack[^2]);  // remainder
    }
}
