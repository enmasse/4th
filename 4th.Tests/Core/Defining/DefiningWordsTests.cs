using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.Defining;

public class DefiningWordsTests
{
    /// <summary>
    /// Intention: Demonstrate CREATE ... DOES> defining a runtime behavior where created object maintains state.
    /// Expected: Invoking the defined word updates and returns internal counter (e.g., increments stored value).
    /// </summary>
    [Fact(Skip = "CREATE ... DOES> not implemented yet")] 
    public void CreateDoes_Basic()
    {
        var forth = new ForthInterpreter();
        // :NONAME CREATE COUNTER 0 , DOES> 1 + DUP TO COUNTER ;
    }

    /// <summary>
    /// Intention: Validate VALUE and TO allow mutable single-cell storage with assignment semantics.
    /// Expected: After "VALUE X 10 TO X X" stack shows current value (10) and updates when reassigned.
    /// </summary>
    [Fact(Skip = "VALUE and TO not implemented yet")] 
    public void ValueAndTo_Assignment()
    {
        var forth = new ForthInterpreter();
        // VALUE X 10 TO X X should yield 10
    }

    /// <summary>
    /// Intention: Ensure DEFER creates a deferred word whose target can be rebound using IS.
    /// Expected: After rebinding, invoking deferred word executes new target definition.
    /// </summary>
    [Fact(Skip = "DEFER and IS not implemented yet")] 
    public void DeferAndIs_Rebinding()
    {
        var forth = new ForthInterpreter();
        // DEFER ACT : HELLO 123 ; ' HELLO IS ACT ACT should push 123
    }

    /// <summary>
    /// Intention: Confirm CONSTANT captures stack top at definition time and always pushes that value.
    /// Expected: "99 CONSTANT N N" leaves 99 on stack consistently.
    /// </summary>
    [Fact(Skip = "CONSTANT word not implemented yet")] 
    public void Constant_Definition()
    {
        var forth = new ForthInterpreter();
        // 99 CONSTANT N  N should push 99
    }
}
