using System.Threading.Tasks;
using Forth;
using Xunit;

namespace Forth.Tests;

public class PublicStackApiTests
{
    [Fact]
    public void PushPopPeekWorks()
    {
        var forth = new ForthInterpreter();
        forth.Push(10L);
        forth.Push(20L);
        Assert.Equal(20L, forth.Peek());
        Assert.Equal(20L, forth.Pop());
        Assert.Equal(10L, forth.Pop());
        var ex = Assert.Throws<ForthException>(() => forth.Pop());
        Assert.Equal(ForthErrorCode.StackUnderflow, ex.Code);
        Assert.Throws<ForthException>(() => forth.Peek());
    }

    [Fact]
    public async Task PushThenEvalAsyncUsesValues()
    {
        var forth = new ForthInterpreter();
        forth.Push(5L);
        forth.Push(7L);
        Assert.True(await forth.EvalAsync("+"));
        Assert.Single(forth.Stack);
        Assert.Equal(12L, (long)forth.Stack[0]);
    }

    private sealed class Custom { public int Value { get; } public Custom(int v) { Value = v; } }

    [Fact]
    public void PushCustomObjectAndRetrieve()
    {
        var forth = new ForthInterpreter();
        var obj = new Custom(42);
        forth.Push(obj);
        Assert.Same(obj, forth.Peek());
        Assert.Same(obj, forth.Pop());
    }
}
