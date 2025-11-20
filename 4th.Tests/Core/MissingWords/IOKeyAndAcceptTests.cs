using Xunit;
using Forth.Core.Interpreter;
using Forth.Core;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Forth.Tests.Core.MissingWords;

public class IOKeyAndAcceptTests
{
    private sealed class TestIO : IForthIO
    {
        private readonly Queue<int> _keys = new();
        private readonly Queue<string?> _lines = new();
        private bool _keyAvailable;

        public readonly List<string> Outputs = new();
        public TestIO(IEnumerable<int>? keys = null, IEnumerable<string?>? lines = null, bool keyAvailable = false)
        {
            if (keys != null) foreach (var k in keys) _keys.Enqueue(k);
            if (lines != null) foreach (var l in lines) _lines.Enqueue(l);
            _keyAvailable = keyAvailable;
        }

        public void Print(string text) => Outputs.Add(text);
        public void PrintNumber(long number) => Outputs.Add(number.ToString());
        public void NewLine() => Outputs.Add("\n");
        public string? ReadLine() => _lines.Count > 0 ? _lines.Dequeue() : null;
        public int ReadKey() => _keys.Count > 0 ? _keys.Dequeue() : -1;
        public bool KeyAvailable() => _keyAvailable || _keys.Count > 0;

        public void SetKeyAvailable(bool v) => _keyAvailable = v;
    }

    [Fact]
    public async Task Key_ReturnsKeyCode()
    {
        var io = new TestIO(keys: new[] { (int)'Q' });
        var forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync("KEY"));
        Assert.Single(forth.Stack);
        Assert.Equal((long)'Q', (long)forth.Stack[0]);
    }

    [Fact]
    public async Task Key_EofReturnsMinusOne()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync("KEY"));
        Assert.Single(forth.Stack);
        Assert.Equal(-1L, (long)forth.Stack[0]);
    }

    [Fact]
    public async Task KeyQuestion_AvailabilityFlags()
    {
        var io1 = new TestIO();
        var f1 = new ForthInterpreter(io1);
        Assert.True(await f1.EvalAsync("KEY?"));
        Assert.Single(f1.Stack);
        Assert.Equal(0L, (long)f1.Stack[0]);

        var io2 = new TestIO(keys: new[] { (int)'X' });
        var f2 = new ForthInterpreter(io2);
        Assert.True(await f2.EvalAsync("KEY?"));
        Assert.Single(f2.Stack);
        Assert.Equal(-1L, (long)f2.Stack[0]);
    }

    [Fact]
    public async Task Accept_ReturnsLineAndLength()
    {
        var io = new TestIO(lines: new[] { "HELLO" });
        var forth = new ForthInterpreter(io);
        // push dummy addr (0) and max length 10
        Assert.True(await forth.EvalAsync("0 10 ACCEPT"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.IsType<string>(forth.Stack[0]);
        Assert.Equal("HELLO", (string)forth.Stack[0]);
        Assert.Equal(5L, (long)forth.Stack[1]);
    }

    [Fact]
    public async Task Expect_ReturnsLineAndLength()
    {
        var io = new TestIO(lines: new[] { "WORLD" });
        var forth = new ForthInterpreter(io);
        // push dummy addr (0) and max length 10
        Assert.True(await forth.EvalAsync("0 10 EXPECT"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.IsType<string>(forth.Stack[0]);
        Assert.Equal("WORLD", (string)forth.Stack[0]);
        Assert.Equal(5L, (long)forth.Stack[1]);
    }
}
