using Forth;
using System;
using System.Linq;
using Xunit;

namespace Forth.Tests;

public class DefinitionAndErrorsTests
{
    private static IForthInterpreter New() => new ForthInterpreter();
    private static long[] Longs(IForthInterpreter f) => f.Stack.Select(o => o is long l ? l : o is int i ? (long)i : 0L).ToArray();

    [Fact]
    public void DefineSimpleWord_ThatAddsTwoNumbers()
    {
        var forth = New();
        Assert.True(forth.Interpret(": ADD2 + ;"));
        Assert.True(forth.Interpret("5 7 ADD2"));
        Assert.Equal(new long[] { 12 }, Longs(forth));
    }

    [Fact]
    public void DefineWord_UsingExistingWords()
    {
        var forth = New();
        Assert.True(forth.Interpret(": SQUARE DUP * ;"));
        Assert.True(forth.Interpret("4 SQUARE"));
        Assert.Equal(new long[] { 16 }, Longs(forth));
    }

    [Fact]
    public void UndefinedWord_ShouldThrow()
    {
        var forth = New();
        Assert.ThrowsAny<Exception>(() => forth.Interpret("FOOBAR"));
    }

    [Fact]
    public void StackUnderflow_ShouldThrow()
    {
        var forth = New();
        Assert.ThrowsAny<Exception>(() => forth.Interpret("+"));
    }
}
