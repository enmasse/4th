using Forth;
using Xunit;

namespace Forth.Tests;

public class VariablesAndLoopsTests
{
    [Fact]
    public void Variable_ReadWrite()
    {
        var forth = new ForthInterpreter();
        Assert.True(forth.Interpret("VARIABLE X"));
        Assert.True(forth.Interpret("10 X !"));
        Assert.True(forth.Interpret("X @"));
        Assert.Equal(new long[] { 10 }, forth.Stack);
    }

    [Fact]
    public void BeginUntil_Loop()
    {
        var forth = new ForthInterpreter();
        Assert.True(forth.Interpret(": INC1 1 + ;"));
        Assert.True(forth.Interpret(": TO5 0 BEGIN INC1 DUP 5 = UNTIL ;"));
        Assert.True(forth.Interpret("TO5"));
        Assert.Equal(new long[] { 5 }, forth.Stack);
    }

    [Fact]
    public void BeginWhileRepeat_Loop()
    {
        var forth = new ForthInterpreter();
        Assert.True(forth.Interpret(": W3 0 BEGIN DUP 3 < WHILE 1 + REPEAT ;"));
        Assert.True(forth.Interpret("W3"));
        Assert.Equal(new long[] { 3 }, forth.Stack);
    }

    [Fact]
    public void CharAndLiteral()
    {
        var forth = new ForthInterpreter();
        Assert.True(forth.Interpret("CHAR A"));
        Assert.Equal(new long[] { 'A' }, forth.Stack);
        Assert.True(forth.Interpret("CHAR Z"));
        Assert.Equal(new long[] { 'A', 'Z' }, forth.Stack);
    }
}
