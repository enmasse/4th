using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core.Defining;

public class DefiningWordsTests
{
    /// <summary>
    /// Intention: Demonstrate CREATE ... DOES> defining a runtime behavior where created object maintains state.
    /// Expected: Invoking the defined word updates and returns internal counter (e.g., increments stored value).
    /// </summary>
    [Fact] 
    public async Task CreateDoes_Basic()
    {
        var forth = new ForthInterpreter();
        // CREATE COUNTER with a cell initialized to 0
        // DOES> reads current value, increments it, stores back, and leaves new value on stack
        Assert.True(await forth.EvalAsync("CREATE COUNTER 0 ,"));
        Assert.True(await forth.EvalAsync("DOES> DUP @ 1 + DUP ROT !"));
        // Invoke twice: 1 then 2
        Assert.True(await forth.EvalAsync("COUNTER"));
        Assert.Single(forth.Stack);
        Assert.Equal(1L, (long)forth.Stack[0]);
        Assert.True(await forth.EvalAsync("COUNTER"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(2L, (long)forth.Stack[1]);
    }

    /// <summary>
    /// Intention: Validate VALUE and TO allow mutable single-cell storage with assignment semantics.
    /// Expected: After "VALUE X 10 TO X X" stack shows current value (10) and updates when reassigned.
    /// </summary>
    [Fact] 
    public async Task ValueAndTo_Assignment()
    {
        var forth = new ForthInterpreter();
        // ANS Forth: VALUE requires an initial value from stack
        Assert.True(await forth.EvalAsync("0 VALUE X")); // define X initialized to 0
        Assert.True(await forth.EvalAsync("10 TO X")); // assign 10
        Assert.True(await forth.EvalAsync("X"));
        Assert.Single(forth.Stack);
        Assert.Equal(10L, (long)forth.Stack[0]);

        // Reassign then fetch
        Assert.True(await forth.EvalAsync("20 TO X X"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(10L, (long)forth.Stack[0]);
        Assert.Equal(20L, (long)forth.Stack[1]);

        // Simplify: reset interpreter to validate reassignment
        var forth2 = new ForthInterpreter();
        Assert.True(await forth2.EvalAsync("5 VALUE X 10 TO X 20 TO X X"));
        Assert.Single(forth2.Stack);
        Assert.Equal(20L, (long)forth2.Stack[0]);
    }

    /// <summary>
    /// Intention: Ensure DEFER creates a deferred word whose target can be rebinding using IS.
    /// Expected: After rebinding, invoking deferred word executes new target definition.
    /// </summary>
    [Fact] 
    public async Task DeferAndIs_Rebinding()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("DEFER ACT : HELLO 123 ; ' HELLO IS ACT ACT"));
        Assert.Single(forth.Stack);
        Assert.Equal(123L, (long)forth.Stack[0]);

        // Rebind to another word
        Assert.True(await forth.EvalAsync(": WORLD 77 ; ' WORLD IS ACT ACT"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(77L, (long)forth.Stack[^1]);
    }

    /// <summary>
    /// Intention: Test DEFER! and DEFER@ for setting and getting the execution token of the most recently defined deferred word.
    /// Expected: DEFER! sets the xt, DEFER@ retrieves it, and the deferred word executes the set xt.
    /// </summary>
    [Fact]
    public async Task DeferStoreAndFetch()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("DEFER MYDEFER : ADD5 5 + ; ' ADD5 DEFER! 10 MYDEFER"));
        Assert.Single(forth.Stack);
        Assert.Equal(15L, (long)forth.Stack[0]);
        forth.Pop(); // clear stack

        // Fetch the xt
        Assert.True(await forth.EvalAsync("DEFER@"));
        Assert.Single(forth.Stack);
        Assert.IsType<Word>(forth.Stack[0]);
        var xt = (Word)forth.Stack[0];
        Assert.Equal("ADD5", xt.Name);
        forth.Pop(); // clear

        // Change to another xt
        Assert.True(await forth.EvalAsync(": SUB3 3 - ; ' SUB3 DEFER! 10 MYDEFER"));
        Assert.Single(forth.Stack);
        Assert.Equal(7L, (long)forth.Stack[0]); // 10 - 3 = 7
    }

    /// <summary>
    /// Intention: Confirm CONSTANT captures stack top at definition time and always pushes that value.
    /// Expected: "99 CONSTANT N N" leaves 99 on stack consistently.
    /// </summary>
    [Fact] 
    public async Task Constant_Definition()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("99 CONSTANT N"));
        Assert.True(await forth.EvalAsync("N N"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(99L, (long)forth.Stack[0]);
        Assert.Equal(99L, (long)forth.Stack[1]);
    }
}
