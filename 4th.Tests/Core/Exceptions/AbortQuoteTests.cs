using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core.Exceptions;

public class AbortQuoteTests
{
    [Fact]
    public async Task AbortQuote_Throws_WhenFlagTrue()
    {
        var f = new ForthInterpreter();
        Assert.True(await f.EvalAsync(": T 1 ABORT\" boom\" ;"));
        var ex = await Assert.ThrowsAsync<Forth.Core.ForthException>(async () => await f.EvalAsync("T"));
        Assert.Equal("boom", ex.Message);
    }

    [Fact]
    public async Task AbortQuote_DoesNotThrow_WhenFlagFalse()
    {
        var f = new ForthInterpreter();
        Assert.True(await f.EvalAsync(": OK 0 ABORT\" boom\" 42 ;"));
        Assert.True(await f.EvalAsync("OK"));
        Assert.Single(f.Stack);
        Assert.Equal(42L, (long)f.Stack[0]);
    }
}
