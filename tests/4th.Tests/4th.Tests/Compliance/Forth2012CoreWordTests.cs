using System.Threading.Tasks;
using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Compliance;

public class Forth2012CoreWordTests
{
    private static ForthInterpreter New() => new();

    [Fact]
    public async Task AndOperations()
    {
        var f = New();
        await f.EvalAsync("0 0 AND");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("0 1 AND");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("1 0 AND");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("1 1 AND");
        Assert.Equal(1L, f.Pop());
    }

    [Fact]
    public async Task OrOperations()
    {
        var f = New();
        await f.EvalAsync("0 0 OR");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("0 1 OR");
        Assert.Equal(1L, f.Pop());

        await f.EvalAsync("1 0 OR");
        Assert.Equal(1L, f.Pop());

        await f.EvalAsync("1 1 OR");
        Assert.Equal(1L, f.Pop());
    }

    [Fact]
    public async Task XorOperations()
    {
        var f = New();
        await f.EvalAsync("0 0 XOR");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("0 1 XOR");
        Assert.Equal(1L, f.Pop());

        await f.EvalAsync("1 0 XOR");
        Assert.Equal(1L, f.Pop());

        await f.EvalAsync("1 1 XOR");
        Assert.Equal(0L, f.Pop());
    }

    [Fact]
    public async Task InvertOperations()
    {
        var f = New();
        await f.EvalAsync("0 INVERT 1 AND");
        Assert.Equal(1L, f.Pop());

        await f.EvalAsync("1 INVERT 1 AND");
        Assert.Equal(0L, f.Pop());
    }

    [Fact]
    public async Task InvertConstants()
    {
        var f = New();
        await f.EvalAsync("0 INVERT");
        var oneS = f.Pop();
        await f.EvalAsync("1 INVERT");
        var zeroS = f.Pop();
        Assert.Equal(oneS, ~0L);
        Assert.Equal(zeroS, ~1L);
    }

    [Fact]
    public async Task StackOperations()
    {
        var f = New();
        await f.EvalAsync("1 2 2DROP");
        Assert.Empty(f.Stack);

        await f.EvalAsync("1 2 2DUP");
        Assert.Equal(4, f.Stack.Count);
        Assert.Equal(2L, f.Pop());
        Assert.Equal(1L, f.Pop());
        Assert.Equal(2L, f.Pop());
        Assert.Equal(1L, f.Pop());

        await f.EvalAsync("1 2 3 4 2OVER");
        Assert.Equal(6, f.Stack.Count);
        Assert.Equal(2L, f.Pop());
        Assert.Equal(1L, f.Pop());
        Assert.Equal(4L, f.Pop());
        Assert.Equal(3L, f.Pop());
        Assert.Equal(2L, f.Pop());
        Assert.Equal(1L, f.Pop());

        await f.EvalAsync("1 2 3 4 2SWAP");
        Assert.Equal(4, f.Stack.Count);
        Assert.Equal(2L, f.Pop());
        Assert.Equal(1L, f.Pop());
        Assert.Equal(4L, f.Pop());
        Assert.Equal(3L, f.Pop());

        await f.EvalAsync("0 ?DUP");
        Assert.Single(f.Stack);
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("1 ?DUP");
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(1L, f.Pop());
        Assert.Equal(1L, f.Pop());

        await f.EvalAsync("DEPTH");
        Assert.Single(f.Stack);
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("0 DEPTH");
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(1L, f.Pop());
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("0 1 DEPTH");
        Assert.Equal(3, f.Stack.Count);
        Assert.Equal(2L, f.Pop());
        Assert.Equal(1L, f.Pop());
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("0 DROP");
        Assert.Empty(f.Stack);

        await f.EvalAsync("1 2 DROP");
        Assert.Single(f.Stack);
        Assert.Equal(1L, f.Pop());

        await f.EvalAsync("1 DUP");
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(1L, f.Pop());
        Assert.Equal(1L, f.Pop());

        await f.EvalAsync("1 2 OVER");
        Assert.Equal(3, f.Stack.Count);
        Assert.Equal(1L, f.Pop());
        Assert.Equal(2L, f.Pop());
        Assert.Equal(1L, f.Pop());

        await f.EvalAsync("1 2 3 ROT");
        Assert.Equal(3, f.Stack.Count);
        Assert.Equal(1L, f.Pop());
        Assert.Equal(3L, f.Pop());
        Assert.Equal(2L, f.Pop());

        await f.EvalAsync("1 2 SWAP");
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(1L, f.Pop());
        Assert.Equal(2L, f.Pop());
    }

    [Fact]
    public async Task ArithmeticOperations()
    {
        var f = New();
        await f.EvalAsync("0 5 +");
        Assert.Equal(5L, f.Pop());

        await f.EvalAsync("5 0 +");
        Assert.Equal(5L, f.Pop());

        await f.EvalAsync("0 -5 +");
        Assert.Equal(-5L, f.Pop());

        await f.EvalAsync("-5 0 +");
        Assert.Equal(-5L, f.Pop());

        await f.EvalAsync("1 2 +");
        Assert.Equal(3L, f.Pop());

        await f.EvalAsync("1 -2 +");
        Assert.Equal(-1L, f.Pop());

        await f.EvalAsync("-1 2 +");
        Assert.Equal(1L, f.Pop());

        await f.EvalAsync("-1 -2 +");
        Assert.Equal(-3L, f.Pop());

        await f.EvalAsync("-1 1 +");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("0 5 -");
        Assert.Equal(-5L, f.Pop());

        await f.EvalAsync("5 0 -");
        Assert.Equal(5L, f.Pop());

        await f.EvalAsync("0 -5 -");
        Assert.Equal(5L, f.Pop());

        await f.EvalAsync("-5 0 -");
        Assert.Equal(-5L, f.Pop());

        await f.EvalAsync("1 2 -");
        Assert.Equal(-1L, f.Pop());

        await f.EvalAsync("1 -2 -");
        Assert.Equal(3L, f.Pop());

        await f.EvalAsync("-1 2 -");
        Assert.Equal(-3L, f.Pop());

        await f.EvalAsync("-1 -2 -");
        Assert.Equal(1L, f.Pop());

        await f.EvalAsync("0 1 -");
        Assert.Equal(-1L, f.Pop());

        await f.EvalAsync("0 1+");
        Assert.Equal(1L, f.Pop());

        await f.EvalAsync("-1 1+");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("1 1+");
        Assert.Equal(2L, f.Pop());

        await f.EvalAsync("2 1-");
        Assert.Equal(1L, f.Pop());

        await f.EvalAsync("1 1-");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("0 1-");
        Assert.Equal(-1L, f.Pop());

        await f.EvalAsync("0 NEGATE");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("1 NEGATE");
        Assert.Equal(-1L, f.Pop());

        await f.EvalAsync("-1 NEGATE");
        Assert.Equal(1L, f.Pop());

        await f.EvalAsync("2 NEGATE");
        Assert.Equal(-2L, f.Pop());

        await f.EvalAsync("-2 NEGATE");
        Assert.Equal(2L, f.Pop());

        await f.EvalAsync("0 ABS");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("1 ABS");
        Assert.Equal(1L, f.Pop());

        await f.EvalAsync("-1 ABS");
        Assert.Equal(1L, f.Pop());

        await f.EvalAsync("0 2*");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("1 2*");
        Assert.Equal(2L, f.Pop());

        await f.EvalAsync("4000 2*");
        Assert.Equal(8000L, f.Pop());

        await f.EvalAsync("0 2/");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("1 2/");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("4000 2/");
        Assert.Equal(2000L, f.Pop());
    }

    [Fact]
    public async Task ComparisonOperations()
    {
        var f = New();
        await f.EvalAsync("0 0=");
        Assert.Equal(-1L, f.Pop());

        await f.EvalAsync("1 0=");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("2 0=");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("-1 0=");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("0 0 =");
        Assert.Equal(-1L, f.Pop());

        await f.EvalAsync("1 1 =");
        Assert.Equal(-1L, f.Pop());

        await f.EvalAsync("-1 -1 =");
        Assert.Equal(-1L, f.Pop());

        await f.EvalAsync("1 0 =");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("-1 0 =");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("0 1 =");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("0 -1 =");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("0 0<");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("-1 0<");
        Assert.Equal(-1L, f.Pop());

        await f.EvalAsync("1 0<");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("0 1 <");
        Assert.Equal(-1L, f.Pop());

        await f.EvalAsync("1 2 <");
        Assert.Equal(-1L, f.Pop());

        await f.EvalAsync("-1 0 <");
        Assert.Equal(-1L, f.Pop());

        await f.EvalAsync("-1 1 <");
        Assert.Equal(-1L, f.Pop());

        await f.EvalAsync("0 0 <");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("1 1 <");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("1 0 <");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("2 1 <");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("0 -1 <");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("1 -1 <");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("0 1 >");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("1 2 >");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("-1 0 >");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("-1 1 >");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("0 0 >");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("1 1 >");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("1 0 >");
        Assert.Equal(-1L, f.Pop());

        await f.EvalAsync("2 1 >");
        Assert.Equal(-1L, f.Pop());

        await f.EvalAsync("0 -1 >");
        Assert.Equal(-1L, f.Pop());

        await f.EvalAsync("1 -1 >");
        Assert.Equal(-1L, f.Pop());

        await f.EvalAsync("0 1 U<");
        Assert.Equal(-1L, f.Pop());

        await f.EvalAsync("1 2 U<");
        Assert.Equal(-1L, f.Pop());

        await f.EvalAsync("0 0 U<");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("1 1 U<");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("1 0 U<");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("2 1 U<");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("0 1 MIN");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("1 2 MIN");
        Assert.Equal(1L, f.Pop());

        await f.EvalAsync("-1 0 MIN");
        Assert.Equal(-1L, f.Pop());

        await f.EvalAsync("-1 1 MIN");
        Assert.Equal(-1L, f.Pop());

        await f.EvalAsync("0 0 MIN");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("1 1 MIN");
        Assert.Equal(1L, f.Pop());

        await f.EvalAsync("1 0 MIN");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("2 1 MIN");
        Assert.Equal(1L, f.Pop());

        await f.EvalAsync("0 -1 MIN");
        Assert.Equal(-1L, f.Pop());

        await f.EvalAsync("1 -1 MIN");
        Assert.Equal(-1L, f.Pop());

        await f.EvalAsync("0 1 MAX");
        Assert.Equal(1L, f.Pop());

        await f.EvalAsync("1 2 MAX");
        Assert.Equal(2L, f.Pop());

        await f.EvalAsync("-1 0 MAX");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("-1 1 MAX");
        Assert.Equal(1L, f.Pop());

        await f.EvalAsync("0 0 MAX");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("1 1 MAX");
        Assert.Equal(1L, f.Pop());

        await f.EvalAsync("1 0 MAX");
        Assert.Equal(1L, f.Pop());

        await f.EvalAsync("2 1 MAX");
        Assert.Equal(2L, f.Pop());

        await f.EvalAsync("0 -1 MAX");
        Assert.Equal(0L, f.Pop());

        await f.EvalAsync("1 -1 MAX");
        Assert.Equal(1L, f.Pop());
    }
}