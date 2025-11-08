using Forth;
using System.Linq;
using Xunit;

namespace Forth.Tests;

public class VariablesAndLoopsTests
{
    private static long[] Longs(IForthInterpreter f) => f.Stack.Select(o => o is long l ? l : o is int i ? (long)i : o is char c ? (long)c : 0L).ToArray();

    [Fact]
    public void Variable_ReadWrite()
    {
        var forth = new ForthInterpreter();
        Assert.True(forth.Interpret("VARIABLE X"));
        Assert.True(forth.Interpret("10 X !"));
        Assert.True(forth.Interpret("X @"));
        Assert.Equal(new long[] { 10 }, Longs(forth));
    }

    [Fact]
    public void BeginUntil_Loop()
    {
        var forth = new ForthInterpreter();
        Assert.True(forth.Interpret(": INC1 1 + ;"));
        Assert.True(forth.Interpret(": TO5 0 BEGIN INC1 DUP 5 = UNTIL ;"));
        Assert.True(forth.Interpret("TO5"));
        Assert.Equal(new long[] { 5 }, Longs(forth));
    }

    [Fact]
    public void BeginWhileRepeat_Loop()
    {
        var forth = new ForthInterpreter();
        Assert.True(forth.Interpret(": W3 0 BEGIN DUP 3 < WHILE 1 + REPEAT ;"));
        Assert.True(forth.Interpret("W3"));
        Assert.Equal(new long[] { 3 }, Longs(forth));
    }

    [Fact]
    public void CharAndLiteral()
    {
        var forth = new ForthInterpreter();
        Assert.True(forth.Interpret("CHAR A"));
        Assert.Equal(new long[] { 'A' }, Longs(forth));
        Assert.True(forth.Interpret("CHAR Z"));
        Assert.Equal(new long[] { 'A', 'Z' }, Longs(forth));
    }
}
