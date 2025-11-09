using Forth;
using Xunit;

namespace Forth.Tests;

public class SyncFastPathTests
{
    /// <summary>
    /// Verifies arithmetic-only line uses fast path and computes correct result.
    /// </summary>
    [Fact]
    public async Task ArithmeticFastPathWorks()
    {
        var f = new ForthInterpreter();
        Assert.True(await f.EvalAsync("5 7 + 2 *"));
        Assert.Single(f.Stack);
        Assert.Equal(24L, (long)f.Stack[0]);
    }

    /// <summary>
    /// Verifies CHAR and quoted string literals are handled by sync IR fast path.
    /// </summary>
    [Fact]
    public async Task StringAndCharFastPathWorks()
    {
        var f = new ForthInterpreter();
        Assert.True(await f.EvalAsync("CHAR A \"hello\""));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(65L, (long)f.Stack[0]);
        Assert.Equal("hello", (string)f.Stack[1]);
    }

    /// <summary>
    /// Ensures a line with AWAIT falls back and surfaces proper error when task missing.
    /// </summary>
    [Fact]
    public async Task FallsBackForAwait()
    {
        var f = new ForthInterpreter();
        var ex = await Assert.ThrowsAsync<ForthException>(async () => await f.EvalAsync("0 AWAIT"));
        Assert.Equal(ForthErrorCode.CompileError, ex.Code);
    }
}
