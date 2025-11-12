using System.Threading.Tasks;
using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.Parsing;

public class DotQuoteTests
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
    public async Task DotQuote_InterpretTime_Prints()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync(".\" HELLO\""));
        Assert.Single(io.Outputs);
        Assert.Equal("HELLO", io.Outputs[0]);
    }

    [Fact]
    public async Task DotQuote_CompileTime_EmitsPrint()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync(": SAY .\" HI\" ; SAY"));
        Assert.Single(io.Outputs);
        Assert.Equal("HI", io.Outputs[0]);
    }

    [Fact]
    public async Task DotQuote_MissingClosing_Throws()
    {
        var forth = new ForthInterpreter();
        await Assert.ThrowsAnyAsync<System.Exception>(() => forth.EvalAsync(".\" OOPS"));
    }
}
