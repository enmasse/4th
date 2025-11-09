using Forth;
using System.Threading.Tasks;
using Xunit;

namespace Forth.Tests;

public class ReplExitTests
{
    /// <summary>
    /// Verifies BYE requests interpreter exit (EvalAsync returns false).
    /// </summary>
    [Fact]
    public async Task Bye_ReturnsFalse()
    {
        var forth = new ForthInterpreter();
        Assert.False(await forth.EvalAsync("BYE"));
    }

    /// <summary>
    /// Verifies QUIT requests interpreter exit (EvalAsync returns false).
    /// </summary>
    [Fact]
    public async Task Quit_ReturnsFalse()
    {
        var forth = new ForthInterpreter();
        Assert.False(await forth.EvalAsync("QUIT"));
    }
}
