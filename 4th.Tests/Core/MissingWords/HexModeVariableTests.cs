using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class HexModeVariableTests
{
    [Fact]
    public async Task Variable_WithHashSign_InHexMode_Works()
    {
        var forth = new ForthInterpreter();
        // Simulate ttester.4th pattern: HEX mode active
        Assert.True(await forth.EvalAsync("HEX"));
        // This should work even in HEX mode
        Assert.True(await forth.EvalAsync("VARIABLE #ERRORS 0 #ERRORS !"));
        Assert.True(await forth.EvalAsync("#ERRORS @"));
        Assert.Single(forth.Stack);
        Assert.Equal(0L, (long)forth.Stack[0]);
        // Clean up
        Assert.True(await forth.EvalAsync("DECIMAL"));
    }

    [Fact]
    public async Task HashErrors_Token_InHexMode_IsWord_NotNumber()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("HEX"));
        // #ERRORS should not be parsed as a number
        // It should fail if it's not defined as a word
        var ex = await Assert.ThrowsAsync<ForthException>(() => forth.EvalAsync("#ERRORS"));
        Assert.Equal(ForthErrorCode.UndefinedWord, ex.Code);
        Assert.True(await forth.EvalAsync("DECIMAL"));
    }
}
