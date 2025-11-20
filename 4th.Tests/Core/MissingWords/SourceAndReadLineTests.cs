using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;
using System.IO;

namespace Forth.Tests.Core.MissingWords;

public class SourceAndReadLineTests
{
    private sealed class TestIO : Forth.Core.IForthIO
    {
        public readonly System.Collections.Generic.List<string> Outputs = new();
        private readonly string _line;
        public TestIO(string line) { _line = line; }
        public void Print(string text) => Outputs.Add(text);
        public void PrintNumber(long number) => Outputs.Add(number.ToString());
        public void NewLine() => Outputs.Add("\n");
        public string? ReadLine() => _line;
    }

    [Fact]
    public async Task Source_PushesCurrentLine()
    {
        var io = new TestIO("hello world");
        var forth = new ForthInterpreter(io);
        // Evaluate SOURCE directly and inspect
        Assert.True(await forth.EvalAsync("SOURCE"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.IsType<string>(forth.Stack[0]);
        Assert.Equal("SOURCE", (string)forth.Stack[0]);
        Assert.Equal(6L, (long)forth.Stack[1]);
    }

    [Fact]
    public async Task ReadLine_WritesMemoryAndReturnsLength()
    {
        var io = new TestIO("abcde");
        var forth = new ForthInterpreter(io);
        // allocate memory at B
        Assert.True(await forth.EvalAsync("CREATE B 16 ALLOT"));
        // call READ-LINE with addr B and u 5
        Assert.True(await forth.EvalAsync("B 5 READ-LINE"));
        // remove the returned length so subsequent C@ checks start from clean stack
        Assert.True(await forth.EvalAsync("DROP"));

        // Verify memory contents at B..B+4 using C@
        Assert.True(await forth.EvalAsync("B C@ B 1 + C@ B 2 + C@ B 3 + C@ B 4 + C@"));
        Assert.Equal(5, forth.Stack.Count);
        Assert.Equal(97L, (long)forth.Stack[0]);
        Assert.Equal(98L, (long)forth.Stack[1]);
        Assert.Equal(99L, (long)forth.Stack[2]);
        Assert.Equal(100L, (long)forth.Stack[3]);
        Assert.Equal(101L, (long)forth.Stack[4]);
    }
}
