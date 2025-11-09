using System.Threading.Tasks;
using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using Forth.ProtoActor;

namespace Forth.Tests;

public class ForthActorTests
{
    /// <summary>
    /// Spawns a Forth interpreter actor and evaluates an addition expression through FORTH-EVAL.
    /// </summary>
    [Fact]
    public async Task SpawnForthAndEvalAdds()
    {
        var f = new ForthInterpreter();
        f.LoadAssemblyWords(typeof(ProtoActorModule).Assembly);
        Assert.True(await f.EvalAsync("USING Proto SPAWN-FORTH"));
        var pid = f.Stack[^1];
        Assert.Equal("Proto.PID", pid.GetType().FullName);
        Assert.True(await f.EvalAsync("USING Proto DUP \"5 7 +\" FORTH-EVAL AWAIT"));
        // After AWAIT, top of stack is response
        var resp = f.Stack[^1];
        Assert.Equal(typeof(Forth.ProtoActor.ForthEvalResponse).FullName, resp.GetType().FullName);
    }
}
