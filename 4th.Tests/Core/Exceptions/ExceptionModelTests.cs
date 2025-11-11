using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core.Exceptions;

public class ExceptionModelTests
{
    /// <summary>
    /// Intention: Validate ANS Forth exception mechanism where CATCH executes an xt and returns error code.
    /// Expected: ABORT or THROW cause nonzero code; THROW rethrows a code to outer handler.
    /// </summary>
    [Fact]
    public async Task CatchThrow_Basic()
    {
        var forth = new ForthInterpreter();
        // Define a word that aborts
        Assert.True(await forth.EvalAsync(": X ABORT \"boom\" ;"));
        // ['] X CATCH -> nonzero error code
        Assert.True(await forth.EvalAsync("' X CATCH"));
        Assert.Single(forth.Stack);
        Assert.NotEqual(0L, (long)forth.Stack[0]);
        var code = (long)forth.Stack[0];
        // THROW rethrows when nonzero
        var ex = await Assert.ThrowsAsync<Forth.Core.ForthException>(async () => await forth.EvalAsync($"{code} THROW"));
        Assert.Equal((Forth.Core.ForthErrorCode)code, ex.Code);
    }

    [Fact]
    public async Task Abort_AbortQuote()
    {
        var forth = new ForthInterpreter();
        var ex = await Assert.ThrowsAsync<Forth.Core.ForthException>(async () => await forth.EvalAsync("ABORT \"failed\""));
        Assert.Equal(Forth.Core.ForthErrorCode.Unknown, ex.Code);
        Assert.Equal("failed", ex.Message);

        var ex2 = await Assert.ThrowsAsync<Forth.Core.ForthException>(async () => await forth.EvalAsync("ABORT"));
        Assert.Equal(Forth.Core.ForthErrorCode.Unknown, ex2.Code);
    }
}
