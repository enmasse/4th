using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Tests.Core.ControlFlow;

public class VariablesAndLoopsTests
{
    [Fact]
    public async Task Variable_ReadWrite()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("VARIABLE X"));
        Assert.True(await forth.EvalAsync("10 X !"));
        Assert.True(await forth.EvalAsync("X @"));
        Assert.Equal(new long[] { 10 }, forth.Stack.Select(o => (long)o).ToArray());
    }

    [Fact]
    public async Task BeginUntil_Loop()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync(": INC1 1 + ;"));
        Assert.True(await forth.EvalAsync(": TO5 0 BEGIN INC1 DUP 5 = UNTIL ;"));
        Assert.True(await forth.EvalAsync("TO5"));
        Assert.Equal(new long[] { 5 }, forth.Stack.Select(o => (long)o).ToArray());
    }

    [Fact]
    public async Task BeginWhileRepeat_Loop()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync(": W3 0 BEGIN DUP 3 < WHILE 1 + REPEAT ;"));
        Assert.True(await forth.EvalAsync("W3"));
        Assert.Equal(new long[] { 3 }, forth.Stack.Select(o => (long)o).ToArray());
    }

    [Fact]
    public async Task CharAndLiteral()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("CHAR A"));
        Assert.Equal(new long[] { 'A' }, forth.Stack.Select(o => (long)o).ToArray());
        Assert.True(await forth.EvalAsync("CHAR Z"));
        Assert.Equal(new long[] { 'A', 'Z' }, forth.Stack.Select(o => (long)o).ToArray());
    }
}
