using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.Stack;

public class ReturnStackTests
{
    /// <summary>
    /// Intention: Verify basic data transfer between data stack and return stack using >R and R>.
    /// Expected: After "1 2 >R R>" the data stack remains 1 2 (R holds and returns 2), matching common Forth semantics.
    /// </summary>
    [Fact] 
    public void ReturnStack_BasicTransfer()
    {
        var forth = new ForthInterpreter();
        Assert.True(forth.Interpret("1 2 >R R>"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(1L, (long)forth.Stack[0]);
        Assert.Equal(2L, (long)forth.Stack[1]);
    }

    /// <summary>
    /// Intention: Validate double-cell transfer with 2>R and 2R> keeps ordering intact for paired cells.
    /// Expected: "10 20 2>R 2R>" restores the original pair on the data stack.
    /// </summary>
    [Fact(Skip = "2>R and 2R> (double-cell transfer) not implemented yet")] 
    public void ReturnStack_DoubleCellTransfer()
    {
        var forth = new ForthInterpreter();
        // forth.Interpret("10 20 2>R 2R>");
    }

    /// <summary>
    /// Intention: Ensure RP@ returns a sensible (implementation-defined) return stack pointer value.
    /// Expected: RP@ pushes an address/integer; primarily used to confirm the word exists and returns a cell.
    /// </summary>
    [Fact(Skip = "RP@ (return stack pointer fetch) not implemented yet")] 
    public void ReturnStack_RPFetch()
    {
        var forth = new ForthInterpreter();
        // forth.Interpret("RP@");
    }
}
