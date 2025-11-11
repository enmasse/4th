using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.Memory;

public class MemoryAndStringTests
{
    /// <summary>
    /// Intention: Verify byte fetch/store operations (C@/C!) operate on single bytes of allocated memory.
    /// Expected: After storing 65 ('A') into buffer, C@ reads back 65 from same address.
    /// </summary>
    [Fact(Skip = "C@ and C! (byte fetch/store) not implemented yet")] 
    public void ByteFetchStore()
    {
        var forth = new ForthInterpreter();
        // CREATE BUF 10 ALLOT  65 BUF C!  BUF C@ should push 65
    }

    /// <summary>
    /// Intention: Validate +! adds a value to a stored cell in place.
    /// Expected: "VARIABLE X 10 X ! 5 X +! X @" yields 15.
    /// </summary>
    [Fact] 
    public void PlusStore()
    {
        var forth = new ForthInterpreter();
        Assert.True(forth.Interpret("VARIABLE X"));
        Assert.True(forth.Interpret("10 X !"));
        Assert.True(forth.Interpret("5 X +!"));
        Assert.True(forth.Interpret("X @"));
        Assert.Single(forth.Stack);
        Assert.Equal(15L, (long)forth.Stack[0]);
    }

    /// <summary>
    /// Intention: Ensure MOVE, FILL, ERASE properly manipulate arbitrary memory regions.
    /// Expected: After operations, destination reflects copied/filled bytes and erased areas become zeroed.
    /// </summary>
    [Fact(Skip = "MOVE FILL ERASE not implemented yet")] 
    public void MoveFillErase()
    {
        var forth = new ForthInterpreter();
        // CREATE A 20 ALLOT CREATE B 20 ALLOT A B 20 MOVE
    }

    /// <summary>
    /// Intention: Verify S" produces an address/length pair and TYPE emits the string.
    /// Expected: Output stream contains the literal text.
    /// </summary>
    [Fact(Skip = "S\" string literal and TYPE not implemented yet")] 
    public void SQuoteType_Output()
    {
        var forth = new ForthInterpreter();
        // S" HELLO" TYPE should print HELLO
    }
}
