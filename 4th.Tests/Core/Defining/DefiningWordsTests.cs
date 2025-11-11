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
    [Fact] 
    public void ValueAndTo_Assignment()
    {
        var forth = new ForthInterpreter();
        Assert.True(forth.Interpret("VALUE X")); // define X default 0
        Assert.True(forth.Interpret("10 TO X")); // assign 10
        Assert.True(forth.Interpret("X"));
        Assert.Single(forth.Stack);
        Assert.Equal(10L, (long)forth.Stack[0]);

        // Reassign then fetch
        Assert.True(forth.Interpret("20 TO X X"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(10L, (long)forth.Stack[0]);
        Assert.Equal(20L, (long)forth.Stack[1]);

        // Simplify: reset interpreter to validate reassignment
        var forth2 = new ForthInterpreter();
        Assert.True(forth2.Interpret("VALUE X 10 TO X 20 TO X X"));
        Assert.Single(forth2.Stack);
        Assert.Equal(20L, (long)forth2.Stack[0]);
    }

    /// <summary>
    /// Intention: Ensure DEFER creates a deferred word whose target can be rebinding using IS.
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
    [Fact] 
    public void Constant_Definition()
    {
        var forth = new ForthInterpreter();
        Assert.True(forth.Interpret("99 CONSTANT N"));
        Assert.True(forth.Interpret("N N"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(99L, (long)forth.Stack[0]);
        Assert.Equal(99L, (long)forth.Stack[1]);
    }
}
