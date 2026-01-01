using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Repl;

public class ReplExitTests
{
    [Fact]
    public async Task ByeAndQuitExit()
    {
        var f = new ForthInterpreter();
        Assert.False(await f.EvalAsync("BYE"));
        f = new ForthInterpreter();
        Assert.False(await f.EvalAsync("QUIT"));
    }
}
