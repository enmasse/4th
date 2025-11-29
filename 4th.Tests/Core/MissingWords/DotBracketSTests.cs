using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class DotBracketSTests
{
    private sealed class TestIO : Forth.Core.IForthIO
    {
        public readonly System.Collections.Generic.List<string> Outputs = new();
        public void Print(string text) => Outputs.Add(text);
        public void PrintNumber(long number) => Outputs.Add(number.ToString());
        public void NewLine() => Outputs.Add("\n");
        public string? ReadLine() => null;
    }

    [Fact]
    public async Task DotBracketS_StackDisplay()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync("1 2 3 .[S]"));
        Assert.Single(io.Outputs);
        Assert.Equal("[3] 1 2 3", io.Outputs[0]);
        Assert.True(forth.Stack.Count == 3); // .[S] must not change the stack
    }

    [Fact]
    public async Task DotBracketS_EmptyStack()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync(".[S]"));
        Assert.Single(io.Outputs);
        Assert.Equal("[0] ", io.Outputs[0]);
        Assert.True(forth.Stack.Count == 0);
    }

    [Fact]
    public async Task DotBracketS_SingleItem()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync("42 .[S]"));
        Assert.Single(io.Outputs);
        Assert.Equal("[1] 42", io.Outputs[0]);
        Assert.True(forth.Stack.Count == 1);
    }
}