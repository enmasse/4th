using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace Forth.Tests;

public class StackAndArithmeticTests
{
    private static IForthInterpreter New() => new ForthInterpreter();
    private static long[] Longs(IForthInterpreter f) => f.Stack.Select(o => o is long l ? l : o is int i ? (long)i : 0L).ToArray();

    [Fact]
    public async Task PushNumbersOntoStack()
    {
        var forth = New();
        Assert.True(await forth.EvalAsync("1 2 3"));
        Assert.Equal(new long[] { 1, 2, 3 }, Longs(forth));
    }

    [Fact]
    public async Task Addition()
    {
        var forth = New();
        Assert.True(await forth.EvalAsync("1 2 +"));
        Assert.Equal(new long[] { 3 }, Longs(forth));
    }

    [Fact]
    public async Task Subtraction()
    {
        var forth = New();
        Assert.True(await forth.EvalAsync("5 2 -"));
        Assert.Equal(new long[] { 3 }, Longs(forth));
    }

    [Fact]
    public async Task Multiplication()
    {
        var forth = New();
        Assert.True(await forth.EvalAsync("4 3 *"));
        Assert.Equal(new long[] { 12 }, Longs(forth));
    }

    [Fact]
    public async Task Division_TruncatesTowardZero()
    {
        var forth = New();
        Assert.True(await forth.EvalAsync("7 2 /"));
        Assert.Equal(new long[] { 3 }, Longs(forth));
    }
}
