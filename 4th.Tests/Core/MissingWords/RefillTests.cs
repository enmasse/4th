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

    [Fact]
    public async Task Refill_ReadsNextLineAndSetsSource()
    {
        var io = new RefillTestIO();
        io.AddLine("hello world");
        io.AddLine("second line");
        var forth = new ForthInterpreter(io);

        // First REFILL SOURCE >IN @
        Assert.True(await forth.EvalAsync("REFILL SOURCE >IN @"));
        Assert.Equal(4, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[0]); // flag
        var addr = (long)forth.Stack[1];
        var len = (long)forth.Stack[2];
        var inVal = (long)forth.Stack[3];
        Assert.Equal(11L, len); // "hello world".Length
        var source = forth.ReadCountedString(addr);
        Assert.Equal("hello world", source);
        Assert.Equal(0L, inVal);

        // Second REFILL SOURCE
        Assert.True(await forth.EvalAsync("REFILL SOURCE"));
        Assert.Equal(7, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[4]); // flag
        addr = (long)forth.Stack[5];
        len = (long)forth.Stack[6];
        Assert.Equal(11L, len); // "second line".Length
        source = forth.ReadCountedString(addr);
        Assert.Equal("second line", source);
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