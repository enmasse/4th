using System.Threading.Tasks;
using Forth;
using Xunit;
using Forth.ProtoActor; // reference for typeof

namespace Forth.Tests;

public class ProtoActorModuleTests
{
    [Fact]
    public async Task LoadProtoActorModuleAndSpawnEcho()
    {
        var f = new ForthInterpreter();
        f.LoadAssemblyWords(typeof(ProtoActorModule).Assembly); // C# load
        Assert.True(await f.EvalAsync("USING Proto SPAWN-ECHO"));
        Assert.Single(f.Stack);
        Assert.Equal("Proto.PID", f.Stack[0].GetType().FullName);
    }

    [Fact]
    public async Task AskLongReturnsValue()
    {
        var f = new ForthInterpreter();
        f.LoadAssemblyWords(typeof(ProtoActorModule).Assembly);
        Assert.True(await f.EvalAsync("USING Proto SPAWN-ECHO DUP 42 ASK-LONG AWAIT"));
        Assert.True(f.Stack.Count >= 2);
        var result = f.Stack[^1];
        Assert.Equal(42L, (long)result);
    }
}
