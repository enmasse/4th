using Forth.Core.Interpreter;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forth.Tests.Core.Introspection;

public class IntrospectionAndToolsTests
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
    /// Intention: Validate .S prints current stack contents for debugging without altering stack state.
    /// Expected: After 1 2 3 .S output contains representation like "<3> 1 2 3".
    /// </summary>
    [Fact]
    public async Task DotS_StackDisplay()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync("1 2 3 .S"));
        Assert.Single(io.Outputs);
        Assert.Equal("<3> 1 2 3", io.Outputs[0]);
        Assert.Equal(3, forth.Stack.Count); // .S must not change the stack
    }

    /// <summary>
    /// Intention: Ensure DEPTH pushes current stack depth count without modifying existing items.
    /// Expected: 1 2 3 DEPTH -> 1 2 3 3.
    /// </summary>
    [Fact] 
    public async Task Depth_Word()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1 2 3 DEPTH"));
        Assert.Equal(4, forth.Stack.Count);
        Assert.Equal(1L, (long)forth.Stack[0]);
        Assert.Equal(2L, (long)forth.Stack[1]);
        Assert.Equal(3L, (long)forth.Stack[2]);
        Assert.Equal(3L, (long)forth.Stack[3]);
    }

    /// <summary>
    /// Intention: Confirm SEE decompiles a word showing its definition for inspection.
    /// Expected: Output includes colon definition for previously defined word.
    /// </summary>
    [Fact]
    public async Task See_Decompile()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync(": X 1 2 + ; SEE X"));
        Assert.Single(io.Outputs);
        Assert.Equal(": X 1 2 + ;", io.Outputs[0]);
    }

    /// <summary>
    /// Intention: Validate DUMP outputs raw memory contents for a given address range.
    /// Expected: Output shows hex/character pairs for requested cell span.
    /// </summary>
    [Fact]
    public async Task Dump_Memory()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        // Allocate buffer and fill with pattern 0..15
        Assert.True(await forth.EvalAsync("CREATE BUF 16 ALLOT"));
        for (int n = 0; n < 16; n++)
        {
            Assert.True(await forth.EvalAsync($"{n} BUF {n} + C!"));
        }
        Assert.True(await forth.EvalAsync("BUF 16 DUMP"));
        Assert.Single(io.Outputs); // SEE output removed; only DUMP line
        Assert.Equal("00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F", io.Outputs[0]);
    }

    /// <summary>
    /// Intention: Validate WORDS lists all known words including built-ins.
    /// Expected: Output contains words like "+" and ".S" showing they are available.
    /// </summary>
    [Fact]
    public async Task Words_PrintsWordList()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync("WORDS"));
        Assert.Single(io.Outputs);
        var outp = io.Outputs[0];
        Assert.Contains("+", outp);
        Assert.Contains(".S", outp);
    }
}
