using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.ControlFlow;

public class DoLoopTests
{
    /// <summary>
    /// Intention: Verify simple DO ... LOOP iteration with I as loop index accumulates correct sum.
    /// Expected: ": SUM10 0 10 0 DO I + LOOP ; SUM10" leaves 45 on the stack (0+1+..+9).
    /// </summary>
    [Fact]
    public void DoLoop_Basic()
    {
        var forth = new ForthInterpreter();
        Assert.True(forth.Interpret(": SUM10 0 10 0 DO I + LOOP ;"));
        Assert.True(forth.Interpret("SUM10"));
        Assert.Single(forth.Stack);
        Assert.Equal(45L, (long)forth.Stack[0]);
    }

    /// <summary>
    /// Intention: Validate LEAVE breaks out early from a loop when a condition is met.
    /// Expected: Accumulator stops updating once index reaches 5, producing 0+1+2+3+4 = 10.
    /// </summary>
    [Fact]
    public void DoLoop_LeaveEarly()
    {
        var forth = new ForthInterpreter();
        // : SUM5 0 10 0 DO I 5 = IF LEAVE THEN I + LOOP ; -> pushes 10
        Assert.True(forth.Interpret(": SUM5 0 10 0 DO I 5 = IF LEAVE THEN I + LOOP ;"));
        Assert.True(forth.Interpret("SUM5"));
        Assert.Single(forth.Stack);
        Assert.Equal(10L, (long)forth.Stack[0]);
    }

    /// <summary>
    /// Intention: Ensure UNLOOP correctly cleans loop parameters when exiting early (e.g., via EXIT).
    /// Expected: No stack corruption or runtime error when exiting loop prematurely.
    /// </summary>
    [Fact(Skip = "UNLOOP not implemented yet")]
    public void Unloop_InsideExit()
    {
        var forth = new ForthInterpreter();
        // : T 0 0 10 DO I 3 = IF UNLOOP EXIT THEN 1 + LOOP ;
    }
}
