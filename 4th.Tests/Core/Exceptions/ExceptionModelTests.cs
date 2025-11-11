using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.Exceptions;

public class ExceptionModelTests
{
    /// <summary>
    /// Intention: Validate ANS Forth exception mechanism where CATCH executes an xt and returns error code.
    /// Expected: ABORT or THROW cause nonzero code; THROW rethrows a code to outer handler.
    /// </summary>
    [Fact(Skip = "CATCH/THROW not implemented yet")] 
    public void CatchThrow_Basic()
    {
        var forth = new ForthInterpreter();
        // : X ABORT" boom" ;
        // ['] X CATCH -> nonzero error code; then THROW to rethrow
    }

    [Fact(Skip = "ABORT and ABORT\" not implemented yet")] 
    public void Abort_AbortQuote()
    {
        var forth = new ForthInterpreter();
        // ABORT" failed" should throw and unwind
    }
}
