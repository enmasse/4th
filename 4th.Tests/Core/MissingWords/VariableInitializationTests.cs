using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class VariableInitializationTests
{
    [Fact]
    public async Task Variable_ThenInitialize_Works()
    {
        var forth = new ForthInterpreter();
        // This is the correct pattern
        Assert.True(await forth.EvalAsync("VARIABLE X"));
        Assert.True(await forth.EvalAsync("0 X !"));
        Assert.True(await forth.EvalAsync("X @"));
        Assert.Single(forth.Stack);
        Assert.Equal(0L, (long)forth.Stack[0]);
    }

    [Fact]
    public async Task Variable_SameLineInitialize_Works()
    {
        var forth = new ForthInterpreter();
        // This pattern SHOULD work: VARIABLE creates word, 0 pushes value, X pushes address, ! stores
        Assert.True(await forth.EvalAsync("VARIABLE Y 0 Y !"));
        Assert.True(await forth.EvalAsync("Y @"));
        Assert.Single(forth.Stack);
        Assert.Equal(0L, (long)forth.Stack[0]);
    }

    [Fact]
    public async Task Variable_SameLineInitialize_NonZero_Works()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("VARIABLE Z 42 Z !"));
        Assert.True(await forth.EvalAsync("Z @"));
        Assert.Single(forth.Stack);
        Assert.Equal(42L, (long)forth.Stack[0]);
    }

    [Fact]
    public async Task Variable_MultipleLines_Works()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("VARIABLE COUNT"));
        Assert.True(await forth.EvalAsync("VARIABLE SUM"));
        Assert.True(await forth.EvalAsync("0 COUNT !"));
        Assert.True(await forth.EvalAsync("0 SUM !"));
        Assert.True(await forth.EvalAsync("COUNT @"));
        Assert.True(await forth.EvalAsync("SUM @"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[0]);
        Assert.Equal(0L, (long)forth.Stack[1]);
    }

    [Fact]
    public async Task Variable_WithHashSign_TtesterPattern_Works()
    {
        var forth = new ForthInterpreter();
        // This is the EXACT pattern from ttester.4th line:
        // VARIABLE #ERRORS 0 #ERRORS !
        Assert.True(await forth.EvalAsync("VARIABLE #ERRORS 0 #ERRORS !"));
        Assert.True(await forth.EvalAsync("#ERRORS @"));
        Assert.Single(forth.Stack);
        Assert.Equal(0L, (long)forth.Stack[0]);
    }

    [Fact]
    public void Tokenizer_HandlesHashInWordName()
    {
        var tokens = Forth.Core.Interpreter.Tokenizer.Tokenize("VARIABLE #ERRORS 0 #ERRORS !");
        Assert.Equal(5, tokens.Count);
        Assert.Equal("VARIABLE", tokens[0]);
        Assert.Equal("#ERRORS", tokens[1]);
        Assert.Equal("0", tokens[2]);
        Assert.Equal("#ERRORS", tokens[3]);
        Assert.Equal("!", tokens[4]);
    }
}
