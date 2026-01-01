using System.Threading.Tasks;
using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using Forth.ProtoActor;

namespace Actor;

public class ForthActorTests
{
    [Fact]
    public async Task SpawnForthAndEvalAdds()
    {
        var f = new ForthInterpreter();
        f.LoadAssemblyWords(typeof(ProtoActorModule).Assembly);
        Assert.True(await f.EvalAsync("USING Proto SPAWN-FORTH"));
        var pid = f.Stack[^1];
        Assert.Equal("Proto.PID", pid.GetType().FullName);
        Assert.True(await f.EvalAsync("USING Proto DUP \"5 7 +\" FORTH-EVAL AWAIT"));
        var resp = f.Stack[^1];
        Assert.Equal(typeof(ForthEvalResponse).FullName, resp.GetType().FullName);
    }
}
