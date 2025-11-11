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
    [Fact] 
    public async Task ByteFetchStore()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("CREATE BUF 10 ALLOT"));
        // Push value 65 then address and store
        Assert.True(await forth.EvalAsync("65 BUF C!"));
        // Fetch should push 65
        Assert.True(await forth.EvalAsync("BUF C@"));
        Assert.Single(forth.Stack);
        Assert.Equal(65L, (long)forth.Stack[0]);
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
    [Fact]
    public async Task MoveFillErase()
    {
        var forth = new ForthInterpreter();
        // Allocate two buffers A and B of 20 bytes
        Assert.True(await forth.EvalAsync("CREATE A 20 ALLOT CREATE B 20 ALLOT"));
        // Fill A with byte value 1
        Assert.True(await forth.EvalAsync("A 20 1 FILL"));
        // Move 20 bytes from A to B
        Assert.True(await forth.EvalAsync("A B 20 MOVE"));
        // Check first and last bytes in B are 1
        Assert.True(await forth.EvalAsync("B C@ B 19 + C@"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(1L, (long)forth.Stack[0]);
        Assert.Equal(1L, (long)forth.Stack[1]);

        // Erase second half of B (bytes 10..19)
        Assert.True(await forth.EvalAsync("B 10 + 10 ERASE"));
        // Check B[0] still 1 and B[19] is 0 now
        Assert.True(await forth.EvalAsync("B C@ B 19 + C@"));
        Assert.Equal(4, forth.Stack.Count);
        Assert.Equal(1L, (long)forth.Stack[2]);
        Assert.Equal(0L, (long)forth.Stack[3]);
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
