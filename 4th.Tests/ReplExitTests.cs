using Forth;
using System.Threading.Tasks;
using Xunit;

namespace Forth.Tests;

public class ReplExitTests
{
    [Fact]
    public async Task Bye_ReturnsFalse()
    {
        var forth = new ForthInterpreter();
        Assert.False(await forth.InterpretAsync("BYE"));
    }

    [Fact]
    public async Task Quit_ReturnsFalse()
    {
        var forth = new ForthInterpreter();
        Assert.False(await forth.InterpretAsync("QUIT"));
    }
}
