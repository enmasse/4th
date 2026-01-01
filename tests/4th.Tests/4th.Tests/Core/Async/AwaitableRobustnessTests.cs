using Xunit;
using Forth.Core.Interpreter;
using System.Threading.Tasks;
using System.Linq;

namespace Forth.Tests.Core.Async;

public class AwaitableRobustnessTests
{
    private static ForthInterpreter New() => new();

    [Fact]
    public async Task Await_TaskOfT_PushesResult()
    {
        var f = New();
        f.AddWord("PUSHTASK", i => i.Push(Task.FromResult<object?>(123)));
        Assert.True(await f.EvalAsync("PUSHTASK AWAIT"));
        Assert.Single(f.Stack);
        Assert.Equal(123L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task Await_ValueTaskOfT_PushesResult()
    {
        var f = New();
        f.AddWord("PUSHVTT", i => i.Push(ValueTask.FromResult<object?>(321)));
        Assert.True(await f.EvalAsync("PUSHVTT AWAIT"));
        Assert.Single(f.Stack);
        Assert.Equal(321L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task Await_ValueTask_Void_Completes()
    {
        var f = New();
        f.AddWord("PUSHV", i => i.Push(new ValueTask(Task.CompletedTask)));
        Assert.True(await f.EvalAsync("PUSHV AWAIT"));
        Assert.Empty(f.Stack);
    }

    [Fact]
    public async Task Await_Throws_ReportsExecutionError()
    {
        var f = New();
        f.AddWord("PUSHT", i => i.Push(Task.FromException(new System.InvalidOperationException("boom"))));
        await Assert.ThrowsAsync<Forth.Core.ForthException>(async () => await f.EvalAsync("PUSHT AWAIT"));
    }
}
