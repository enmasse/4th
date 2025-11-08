using System.Threading.Tasks;
using Forth;
using Xunit;
using System.Linq;

namespace Forth.Tests;

public class FiberSpawnTests
{
    private static ForthInterpreter New() => new();
    private static long[] Longs(ForthInterpreter f) => f.Stack.Select(o => o is long l ? l : 0L).ToArray();

    [Fact]
    public async Task SpawnDuplicateTaskAndAwaitBoth()
    {
        var f = New();
        await f.InterpretAsync("BINDASYNC Forth.Tests.AsyncTestTargets AddAsync 2 ADDAB 10 32 ADDAB DUP SPAWN AWAIT AWAIT");
        Assert.Equal(new long[]{42}, Longs(f));
    }

    [Fact]
    public async Task YieldDoesNotLoseStack()
    {
        var f = New();
        await f.InterpretAsync("1 2 3 YIELD 4 5");
        Assert.Equal(new long[]{1,2,3,4,5}, Longs(f));
    }
}
