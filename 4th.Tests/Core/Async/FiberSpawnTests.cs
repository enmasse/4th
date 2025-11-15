using System.Linq;
using System.Threading.Tasks;
using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.Async;

public class FiberSpawnTests
{
    private static ForthInterpreter New() => new();

    [Fact]
    public async Task SpawnDuplicateTaskAndAwaitBoth()
    {
        var f = New();
        await f.EvalAsync("BIND Forth.Tests.Core.Binding.AsyncTestTargets AddAsync 2 ADDAB");
        await f.EvalAsync("10 32 ADDAB AWAIT");
        Assert.Equal(42, f.Pop());
    }

    [Fact]
    public async Task YieldDoesNotLoseStack()
    {
        var f = New();
        await f.EvalAsync("1 2 3 YIELD 4 5");
        Assert.Equal(new long[]{1,2,3,4,5}, f.Stack.Select(o => (long)o).ToArray());
    }
}
