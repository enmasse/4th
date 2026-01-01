using System.Threading.Tasks;
using Xunit;
using Forth.Core.Interpreter;

namespace Forth.Tests.Compliance;

public class BracketIfConsumptionTest
{
    [Fact]
    public async Task BracketIF_ConsumesToCommentOut()
    {
        var forth = new ForthInterpreter();
        // Test that [IF] with false condition comments out/skips the rest of the line
        Assert.True(await forth.EvalAsync("0 [IF] UNDEFINED-WORD [THEN]"));
        Assert.Empty(forth.Stack);
        
        // Test that it works with true condition
        forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1 [IF] 42 [THEN]"));
        Assert.Single(forth.Stack);
        Assert.Equal(42L, (long)forth.Stack[0]);
    }
}
