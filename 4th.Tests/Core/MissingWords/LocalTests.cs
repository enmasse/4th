using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class LocalTests
{
    [Fact]
    public async Task LocalsBar_DeclaresLocals()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync(": TEST 1 2 3 LOCALS| c b a | a b + c * . ;"));
        Assert.True(await forth.EvalAsync("TEST"));
        Assert.Single(io.Outputs);
        Assert.Equal("9", io.Outputs[0]);
    }

    [Fact]
    public async Task Local_DeclaresLocal()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync(": TEST 1 2 (LOCAL) b b . ;"));
        Assert.True(await forth.EvalAsync("TEST"));
        Assert.Single(io.Outputs);
        Assert.Equal("2", io.Outputs[0]);
    }

    private sealed class TestIO : IForthIO
    {
        public readonly System.Collections.Generic.List<string> Outputs = new();
        public void Print(string text) => Outputs.Add(text);
        public void PrintNumber(long number) => Outputs.Add(number.ToString());
        public void NewLine() => Outputs.Add("\n");
        public string? ReadLine() => null;
    }
}