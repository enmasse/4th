using Forth;
using Xunit;

namespace Forth.Tests;

public class ReplExitTests
{
    [Fact]
    public void Bye_ReturnsFalse()
    {
        var forth = new ForthInterpreter();
        Assert.False(forth.Interpret("BYE"));
    }

    [Fact]
    public void Quit_ReturnsFalse()
    {
        var forth = new ForthInterpreter();
        Assert.False(forth.Interpret("QUIT"));
    }
}
