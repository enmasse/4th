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

    [Fact]
    public async Task Spawn_CapturesParentContext_CanExecuteParentWords()
    {
        var forth = New();
        // Define words in parent that will be used by spawned task
        await forth.EvalAsync(": DOUBLE 2 * ;");
        await forth.EvalAsync(": CALC 5 DOUBLE ;");
        
        // FUTURE should capture parent context and be able to execute CALC
        await forth.EvalAsync("' CALC FUTURE JOIN");
        
        Assert.Single(forth.Stack);
        Assert.Equal(10L, (long)forth.Stack[0]);
    }

    [Fact]
    public async Task Spawn_CapturesVariablesAndConstants()
    {
        var forth = New();
        // Define variable and constant in parent
        await forth.EvalAsync("42 CONSTANT ANSWER");
        await forth.EvalAsync(": GET-ANSWER ANSWER ;");
        
        // Spawned task should see ANSWER constant
        await forth.EvalAsync("' GET-ANSWER FUTURE JOIN");
        
        Assert.Single(forth.Stack);
        Assert.Equal(42L, (long)forth.Stack[0]);
    }

    [Fact]
    public async Task Spawn_CapturesModuleContext()
    {
        var forth = New();
        // Define module with word in parent
        await forth.EvalAsync("MODULE TestMod : ADD3 3 + ; END-MODULE");
        await forth.EvalAsync("USING TestMod");
        await forth.EvalAsync(": USE-ADD3 10 ADD3 ;");
        
        // Spawned task should have access to module
        await forth.EvalAsync("' USE-ADD3 FUTURE JOIN");
        
        Assert.Single(forth.Stack);
        Assert.Equal(13L, (long)forth.Stack[0]);
    }
}
