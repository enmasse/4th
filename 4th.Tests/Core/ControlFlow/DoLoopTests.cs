using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.ControlFlow;

public class DoLoopTests
{
    /// <summary>
    /// Intention: Verify simple DO ... LOOP iteration with I as loop index accumulates correct sum.
    /// Expected: ": SUM10 0 10 0 DO I + LOOP ; SUM10" leaves 45 on the stack (0+1+..+9).
    /// </summary>
    [Fact(Skip = "DO/LOOP not implemented yet")] 
    public void DoLoop_Basic()
    {
        var forth = new ForthInterpreter();
        // : SUM10 0 10 0 DO I + LOOP ;  SUM10  should push 45
        // forth.Interpret(": SUM10 0 10 0 DO I + LOOP ;");
        // forth.Interpret("SUM10");
    }

    /// <summary>
    /// Intention: Validate +LOOP stepping and LEAVE to break out early from a loop.
    /// Expected: Loop exits when I=5 via LEAVE and accumulator reflects iterations before exit.
    /// </summary>
    [Fact(Skip = "+LOOP and LEAVE not implemented yet")] 
    public void DoPlusLoop_AndLeave()
    {
        var forth = new ForthInterpreter();
        // : T 0 0 10 DO I 5 = IF LEAVE THEN 1 + LOOP ;
        // forth.Interpret(": T 0 0 10 DO I 5 = IF LEAVE THEN 1 + LOOP ;");
        // forth.Interpret("T");
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
