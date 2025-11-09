using System.Threading.Tasks;
using Forth;
using Xunit;

namespace Forth.Tests;

public class PublicWordApiTests
{
    /// <summary>
    /// Verifies a synchronously added word consumes two numbers and pushes their sum.
    /// </summary>
    [Fact]
    public async Task AddSyncWord_AddsTwoNumbers()
    {
        var forth = new ForthInterpreter();
        forth.AddWord("ADD2", i => {
            var b = (long)i.Pop();
            var a = (long)i.Pop();
            i.Push(a + b);
        });
        Assert.True(await forth.EvalAsync("5 7 ADD2"));
        Assert.Equal(12L, (long)forth.Stack[^1]);
    }

    /// <summary>
    /// Verifies an async word is awaited and its result is pushed.
    /// </summary>
    [Fact]
    public async Task AddAsyncWord_AwaitsAndPushesResult()
    {
        var forth = new ForthInterpreter();
        forth.AddWordAsync("INCASYNC", async i => {
            await Task.Delay(1);
            var a = (long)i.Pop();
            i.Push(a + 1);
        });
        Assert.True(await forth.EvalAsync("41 INCASYNC"));
        Assert.Equal(42L, (long)forth.Stack[^1]);
    }

    /// <summary>
    /// Verifies another sync word using stack helper pattern (SQUARE) works.
    /// </summary>
    [Fact]
    public async Task AddSyncWord_ThatUsesInterpreterStackHelpers()
    {
        var forth = new ForthInterpreter();
        forth.AddWord("SQUARE", i => {
            var a = (long)i.Pop();
            i.Push(a * a);
        });
        Assert.True(await forth.EvalAsync("9 SQUARE"));
        Assert.Equal(81L, (long)forth.Stack[^1]);
    }
}
