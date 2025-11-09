using Forth;
using Xunit;

namespace Forth.Tests;

public class SyncFastPathTests
{
    [Fact]
    public async void ArithmeticFastPathWorks()
    {
        var f = new ForthInterpreter();
        Assert.True(await f.EvalAsync("5 7 + 2 *"));
        Assert.Single(f.Stack);
        Assert.Equal(24L, (long)f.Stack[0]);
    }

    [Fact]
    public async void StringAndCharFastPathWorks()
    {
        var f = new ForthInterpreter();
        Assert.True(await f.EvalAsync("CHAR A \"hello\""));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(65L, (long)f.Stack[0]);
        Assert.Equal("hello", (string)f.Stack[1]);
    }

    [Fact]
    public async void FallsBackForAwait()
    {
        var f = new ForthInterpreter();
        // Should hit fallback and throw since AWAIT needs task; ensure normal behavior preserved
        var ex = await Assert.ThrowsAsync<ForthException>(async () => await f.EvalAsync("0 AWAIT"));
        Assert.Equal(ForthErrorCode.CompileError, ex.Code);
    }
}
