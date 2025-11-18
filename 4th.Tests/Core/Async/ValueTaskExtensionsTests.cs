using System.Threading.Tasks;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;

namespace Forth.Tests.Core.Async;

public class ValueTaskExtensionsTests
{
    private static ForthInterpreter New() => new();
    private static long[] Longs(ForthInterpreter f) => f.Stack.Select(o => o is long l ? l : o is int i ? (long)i : 0L).ToArray();

    [Fact]
    public async Task Await_DirectValueTaskT_Completed()
    {
        var f = New();
        // Push a boxed ValueTask<int> directly
        f.AddWord("PUSHVT", i => i.Push(ValueTask.FromResult(42)));
        Assert.True(await f.EvalAsync("PUSHVT AWAIT"));
        Assert.Single(f.Stack);
        Assert.Equal(42L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task Await_DirectValueTask_Void()
    {
        var f = New();
        // Push a boxed non-generic ValueTask wrapping a completed Task
        f.AddWord("PUSHVT", i => i.Push(new ValueTask(Task.CompletedTask)));
        Assert.True(await f.EvalAsync("PUSHVT AWAIT"));
        Assert.Empty(f.Stack);
    }

    [Fact]
    public async Task Taskq_ValueTask_IncompleteThenComplete()
    {
        var f = New();
        var tcs = new TaskCompletionSource<int>();
        // Push a ValueTask<int> backed by a Task we control
        f.AddWord("PUSHVT", i => i.Push(new ValueTask<int>(tcs.Task)));

        // Initially not completed
        Assert.True(await f.EvalAsync("PUSHVT TASK?"));
        Assert.Single(f.Stack);
        Assert.Equal(0L, (long)f.Stack[0]);

        // Push again, complete, then check
        Assert.True(await f.EvalAsync("PUSHVT"));
        tcs.SetResult(7);
        Assert.True(await f.EvalAsync("TASK?"));
        Assert.Equal(1L, (long)f.Stack[^1]);
    }
}
