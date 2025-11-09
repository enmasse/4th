using Forth.Core;
using Forth.Core.Interpreter;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Forth.Tests;

public class ControlFlowAndIOPlan
{
    private sealed class TestIO : IForthIO
    {
        public readonly List<string> Outputs = new();
        public void Print(string text) => Outputs.Add(text);
        public void PrintNumber(long number) => Outputs.Add(number.ToString());
        public void NewLine() => Outputs.Add("\n");
        public string? ReadLine() => null;
    }

    private static long[] Longs(IForthInterpreter f) => f.Stack.Select(o => o is long l ? l : o is int i ? (long)i : 0L).ToArray();

    [Fact]
    public async Task IfElseThen_Branching()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync(": FLOOR5 DUP 6 < IF DROP 5 ELSE 1 - THEN ;"));
        Assert.True(await forth.EvalAsync("1 FLOOR5"));
        Assert.Equal(new long[] { 5 }, Longs(forth));
        Assert.True(await forth.EvalAsync("8 FLOOR5"));
        Assert.Equal(new long[] { 5, 7 }, Longs(forth));
    }

    [Fact]
    public async Task Output_NumberAndNewline()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync("25 10 * 50 + CR ."));
        Assert.Equal(new[] { "\n", "300" }, io.Outputs);
    }
}
