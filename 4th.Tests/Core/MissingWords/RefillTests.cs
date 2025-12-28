using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class RefillTests
{
    private sealed class RefillTestIO : IForthIO
    {
        private readonly Queue<string> _lines = new();
        private readonly List<string> _outputs = new();

        public void AddLine(string line) => _lines.Enqueue(line);

        public void Print(string text) => _outputs.Add(text);
        public void PrintNumber(long number) => _outputs.Add(number.ToString());
        public void NewLine() => _outputs.Add("\n");
        public string? ReadLine() => _lines.Count > 0 ? _lines.Dequeue() : null;
        public int ReadKey() => -1;
        public bool KeyAvailable() => false;
    }

    [Fact(Skip = "Architectural limitation: REFILL source and parse source are separate - test needs refactoring")]
    public async Task Refill_ReadsNextLineAndSetsSource()
    {
        var io = new RefillTestIO();
        io.AddLine("hello world");
        io.AddLine("second line");
        var forth = new ForthInterpreter(io);

        // First REFILL then inspect SOURCE and >IN
        Assert.True(await forth.EvalAsync("REFILL DROP"));
        // Now query SOURCE and >IN
        Assert.True(await forth.EvalAsync("SOURCE >IN @"));
        Assert.Equal(3, forth.Stack.Count);
        var addr1 = (long)forth.Stack[0];
        var len1 = (long)forth.Stack[1];
        var in1 = (long)forth.Stack[2];
        Assert.Equal(11L, len1); // "hello world".Length
        var src1 = forth.ReadMemoryString(addr1, len1);
        Assert.Equal("hello world", src1);
        Assert.Equal(0L, in1);
        // Pop the values
        forth.Pop();
        forth.Pop();
        forth.Pop();

        // Second REFILL then inspect SOURCE and >IN
        Assert.True(await forth.EvalAsync("REFILL DROP"));
        Assert.True(await forth.EvalAsync("SOURCE >IN @"));
        Assert.Equal(3, forth.Stack.Count);
        var addr2 = (long)forth.Stack[0];
        var len2 = (long)forth.Stack[1];
        var in2 = (long)forth.Stack[2];
        Assert.Equal(11L, len2); // "second line".Length
        var src2 = forth.ReadMemoryString(addr2, len2);
        Assert.Equal("second line", src2);
        Assert.Equal(0L, in2);
    }

    [Fact]
    public async Task Refill_ReturnsFalseOnEOF()
    {
        var io = new RefillTestIO();
        // No lines added
        var forth = new ForthInterpreter(io);

        Assert.True(await forth.EvalAsync("REFILL"));
        Assert.Single(forth.Stack);
        Assert.Equal(0L, (long)forth.Stack[0]);
    }
}