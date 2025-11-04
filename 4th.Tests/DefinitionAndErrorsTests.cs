using Forth;
using System;
using Xunit;

namespace Forth.Tests;

public class DefinitionAndErrorsTests
{
    private static IForthInterpreter New() => new ForthInterpreter();

    [Fact]
    public void DefineSimpleWord_ThatAddsTwoNumbers()
    {
        var forth = New();
        Assert.True(forth.Interpret(": ADD2 + ;"));
        Assert.True(forth.Interpret("5 7 ADD2"));
        Assert.Equal(new long[] { 12 }, forth.Stack);
    }

    [Fact]
    public void DefineWord_UsingExistingWords()
    {
        var forth = New();
        Assert.True(forth.Interpret(": SQUARE DUP * ;"));
        Assert.True(forth.Interpret("4 SQUARE"));
        Assert.Equal(new long[] { 16 }, forth.Stack);
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
