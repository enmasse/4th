using Forth;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Forth.Tests;

public class MoreWordsTests
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
    public async Task Comparisons_Work()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.InterpretAsync("1 2 <"));
        Assert.Equal(new long[] { 1 }, Longs(forth));
        Assert.True(await forth.InterpretAsync("2 2 ="));
        Assert.Equal(new long[] { 1, 1 }, Longs(forth));
        Assert.True(await forth.InterpretAsync("3 2 >"));
        Assert.Equal(new long[] { 1, 1, 1 }, Longs(forth));
    }

    [Fact]
    public async Task Rotations_Work()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.InterpretAsync("1 2 3 ROT"));
        Assert.Equal(new long[] { 2, 3, 1 }, Longs(forth));
        Assert.True(await forth.InterpretAsync("4 5 -ROT"));
        Assert.Equal(new long[] { 2, 3, 5, 1, 4 }, Longs(forth));
    }

    [Fact]
    public async Task Constant_DefinesValueWord()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.InterpretAsync("42 CONSTANT X"));
        Assert.True(await forth.InterpretAsync("X X +"));
        Assert.Equal(new long[] { 84 }, Longs(forth));
    }

    [Fact]
    public async Task Emit_PrintsCharacter()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        Assert.True(await forth.InterpretAsync("81 EMIT"));
        Assert.Equal(new[] { "Q" }, io.Outputs);
    }
}
