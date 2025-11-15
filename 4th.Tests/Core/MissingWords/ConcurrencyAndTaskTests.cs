using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class ConcurrencyAndTaskTests
{
    private static ForthInterpreter New() => new();
    private static long[] Longs(ForthInterpreter f) => f.Stack.Select(o => o is long l ? l : o is int i ? (long)i : 0L).ToArray();

    [Fact]
    public async Task SpawnAndJoin_CompletesWithoutResult()
    {
        var forth = New();
        // Define a simple word to execute; it won't affect parent interpreter
        await forth.EvalAsync(": NOP ;");
        // SPAWN returns a Task; JOIN should await it and leave no result
        await forth.EvalAsync("' NOP SPAWN JOIN");
        Assert.Empty(forth.Stack);
    }

    [Fact]
    public async Task FutureAndJoin_ReturnsValueFromChild()
    {
        var forth = New();
        await forth.EvalAsync(": PUSH7 7 ;");
        await forth.EvalAsync("' PUSH7 FUTURE JOIN");
        Assert.Equal(new long[]{7}, Longs(forth));
    }

    [Fact]
    public async Task TaskAlias_ReturnsValueFromChild()
    {
        var forth = New();
        await forth.EvalAsync(": PUSH5 5 ;");
        await forth.EvalAsync("' PUSH5 TASK JOIN");
        Assert.Equal(new long[]{5}, Longs(forth));
    }
}
