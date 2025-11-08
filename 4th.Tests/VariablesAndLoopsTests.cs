using Forth;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Forth.Tests;

public class VariablesAndLoopsTests
{
    private static long[] Longs(IForthInterpreter f) => f.Stack.Select(o => o is long l ? l : o is int i ? (long)i : o is char c ? (long)c : 0L).ToArray();

    [Fact]
    public async Task Variable_ReadWrite()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.InterpretAsync("VARIABLE X"));
        Assert.True(await forth.InterpretAsync("10 X !"));
        Assert.True(await forth.InterpretAsync("X @"));
        Assert.Equal(new long[] { 10 }, Longs(forth));
    }

    [Fact]
    public async Task BeginUntil_Loop()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.InterpretAsync(": INC1 1 + ;"));
        Assert.True(await forth.InterpretAsync(": TO5 0 BEGIN INC1 DUP 5 = UNTIL ;"));
        Assert.True(await forth.InterpretAsync("TO5"));
        Assert.Equal(new long[] { 5 }, Longs(forth));
    }

    [Fact]
    public async Task BeginWhileRepeat_Loop()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.InterpretAsync(": W3 0 BEGIN DUP 3 < WHILE 1 + REPEAT ;"));
        Assert.True(await forth.InterpretAsync("W3"));
        Assert.Equal(new long[] { 3 }, Longs(forth));
    }

    [Fact]
    public async Task CharAndLiteral()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.InterpretAsync("CHAR A"));
        Assert.Equal(new long[] { 'A' }, Longs(forth));
        Assert.True(await forth.InterpretAsync("CHAR Z"));
        Assert.Equal(new long[] { 'A', 'Z' }, Longs(forth));
    }
}
