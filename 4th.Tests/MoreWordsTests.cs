using Forth;
using System.Collections.Generic;
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

    [Fact]
    public void Comparisons_Work()
    {
        var forth = new ForthInterpreter();
        Assert.True(forth.Interpret("1 2 <"));
        Assert.Equal(new long[] { 1 }, forth.Stack);
        Assert.True(forth.Interpret("2 2 ="));
        Assert.Equal(new long[] { 1, 1 }, forth.Stack);
        Assert.True(forth.Interpret("3 2 >"));
        Assert.Equal(new long[] { 1, 1, 1 }, forth.Stack);
    }

    [Fact]
    public void Rotations_Work()
    {
        var forth = new ForthInterpreter();
        Assert.True(forth.Interpret("1 2 3 ROT")); // (1 2 3 -- 2 3 1)
        Assert.Equal(new long[] { 2, 3, 1 }, forth.Stack);
        Assert.True(forth.Interpret("4 5 -ROT")); // push 4 5 -> stack: 2 3 1 4 5 -> -ROT on top 3 items (1 4 5 -- 5 1 4)
        Assert.Equal(new long[] { 2, 3, 5, 1, 4 }, forth.Stack);
    }

    [Fact]
    public void Constant_DefinesValueWord()
    {
        var forth = new ForthInterpreter();
        Assert.True(forth.Interpret("42 CONSTANT X"));
        Assert.True(forth.Interpret("X X +"));
        Assert.Equal(new long[] { 84 }, forth.Stack);
    }

    [Fact]
    public void Emit_PrintsCharacter()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        Assert.True(forth.Interpret("81 EMIT")); // 'Q'
        Assert.Equal(new[] { "Q" }, io.Outputs);
    }
}
