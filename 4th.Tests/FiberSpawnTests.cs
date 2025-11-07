using System.Threading.Tasks;
using Forth;
using Xunit;

namespace Forth.Tests;

public class FiberSpawnTests
{
    private static ForthInterpreter New() => new();

    [Fact]
    public async Task SpawnDuplicateTaskAndAwaitBoth()
    {
        var f = New();
        // Create async task handle
        await f.InterpretAsync("BINDASYNC Forth.Tests.AsyncTestTargets AddAsync 2 ADDAB 10 32 ADDAB DUP SPAWN AWAIT AWAIT");
        // First await produced result 42; second await on spawned duplicate nothing left except result once
        Assert.Equal(new long[]{42}, f.Stack);
    }

    [Fact]
    public async Task YieldDoesNotLoseStack()
    {
        var f = New();
        await f.InterpretAsync("1 2 3 YIELD 4 5");
        Assert.Equal(new long[]{1,2,3,4,5}, f.Stack);
    }
}
