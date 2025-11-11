using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Repl;

public class ReplExitTests
{
    [Fact]
    public void ByeAndQuitExit()
    {
        var f = new ForthInterpreter();
        Assert.False(f.Interpret("BYE"));
        f = new ForthInterpreter();
        Assert.False(f.Interpret("QUIT"));
    }
}
