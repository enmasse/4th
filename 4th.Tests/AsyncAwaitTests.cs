using System.Threading.Tasks;
using Forth;
using Xunit;
using System.Linq;

namespace Forth.Tests;

public class AsyncAwaitTests
{
    private static ForthInterpreter New() => new();

    private static long[] Longs(ForthInterpreter f) => f.Stack.Select(o => o is long l ? l : o is int i ? (long)i : 0L).ToArray();

    [Fact]
    public async Task AwaitBoundAsyncMethod()
    {
        var f = New();
        await f.EvalAsync("BINDASYNC Forth.Tests.AsyncTestTargets AddAsync 2 ADDAB 5 7 ADDAB AWAIT");
        Assert.Equal(new long[]{12}, Longs(f));
    }

    [Fact]
    public async Task PollTaskCompletion()
    {
        var f = New();
        await f.EvalAsync("BINDASYNC Forth.Tests.AsyncTestTargets VoidDelay 1 DELAY 30 DELAY DUP TASK? DROP AWAIT");
        Assert.Empty(f.Stack); // void task leaves nothing
    }

    [Fact]
    public async Task AwaitBoundValueTaskMethod()
    {
        var f = New();
        await f.EvalAsync("BINDASYNC Forth.Tests.AsyncTestTargets AddValueTask 2 ADDVT 2 3 ADDVT AWAIT");
        Assert.Equal(new long[]{5}, Longs(f));
    }

    [Fact]
    public async Task AwaitBoundValueTaskVoid()
    {
        var f = New();
        await f.EvalAsync("BINDASYNC Forth.Tests.AsyncTestTargets VoidDelayValueTask 1 DVT 10 DVT AWAIT");
        Assert.Empty(f.Stack);
    }
}
