using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.ControlFlow;

public class DoLoopTests
{
    [Fact(Skip = "DO/LOOP not implemented yet")] 
    public void DoLoop_Basic()
    {
        var forth = new ForthInterpreter();
        // : SUM10 0 10 0 DO I + LOOP ;  SUM10  should push 45
        // forth.Interpret(": SUM10 0 10 0 DO I + LOOP ;");
        // forth.Interpret("SUM10");
    }

    [Fact(Skip = "+LOOP and LEAVE not implemented yet")] 
    public void DoPlusLoop_AndLeave()
    {
        var forth = new ForthInterpreter();
        // : T 0 0 10 DO I 5 = IF LEAVE THEN 1 + LOOP ;
        // forth.Interpret(": T 0 0 10 DO I 5 = IF LEAVE THEN 1 + LOOP ;");
        // forth.Interpret("T");
    }

    [Fact(Skip = "UNLOOP not implemented yet")] 
    public void Unloop_InsideExit()
    {
        var forth = new ForthInterpreter();
        // : T 0 0 10 DO I 3 = IF UNLOOP EXIT THEN 1 + LOOP ;
    }
}
