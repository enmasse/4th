using Forth.Core.Interpreter;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forth.Tests.Core.Memory;

public class MemoryAndStringTests
{
    private sealed class TestIO : Forth.Core.IForthIO
    {
        public readonly List<string> Outputs = new();
        public void Print(string text) => Outputs.Add(text);
        public void PrintNumber(long number) => Outputs.Add(number.ToString());
        public void NewLine() => Outputs.Add("\n");
        public string? ReadLine() => null;
    }

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
    public async Task PlusStore()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("VARIABLE X"));
        Assert.True(await forth.EvalAsync("10 X !"));
        Assert.True(await forth.EvalAsync("5 X +!"));
        Assert.True(await forth.EvalAsync("X @"));
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
    [Fact] 
    public async Task SQuoteType_Output()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync("\"HELLO\" TYPE"));
        Assert.Single(io.Outputs);
        Assert.Equal("HELLO", io.Outputs[0]);
        Assert.Empty(forth.Stack); // TYPE should consume the string
    }
}
