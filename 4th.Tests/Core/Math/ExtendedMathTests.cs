using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core.Math;

public class ExtendedMathTests
{
    /// <summary>
    /// Intention: Provide coverage for core bitwise words to ensure correct logical operations on cells.
    /// Expected: 6 3 AND -> 2; 6 1 OR -> 7; 6 3 XOR -> 5; 0 INVERT -> -1 (two's complement).
    /// </summary>
    [Fact] 
    public async Task Bitwise_Basics()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("6 3 AND"));
        Assert.Equal(2L, (long)forth.Stack[^1]);
        Assert.True(await forth.EvalAsync("6 1 OR"));
        Assert.Equal(7L, (long)forth.Stack[^1]);
        Assert.True(await forth.EvalAsync("6 3 XOR"));
        Assert.Equal(5L, (long)forth.Stack[^1]);
        Assert.True(await forth.EvalAsync("0 INVERT"));
        Assert.Equal(-1L, (long)forth.Stack[^1]);
    }

    /// <summary>
    /// Intention: Validate arithmetic shifts with LSHIFT and RSHIFT on unsigned semantics per standard.
    /// Expected: 1 3 LSHIFT -> 8; 8 2 RSHIFT -> 2.
    /// </summary>
    [Fact] 
    public async Task Bitwise_Shifts()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1 3 LSHIFT"));
        Assert.Equal(8L, (long)forth.Stack[^1]);
        Assert.True(await forth.EvalAsync("8 2 RSHIFT"));
        Assert.Equal(2L, (long)forth.Stack[^1]);
    }

    /// <summary>
    /// Intention: Exercise double-cell stack ops to ensure paired-cell ordering is preserved.
    /// Expected: 1 2 2DUP -> 1 2 1 2; then 2SWAP exchanges pairs.
    /// </summary>
    [Fact] 
    public async Task DoubleCell_Ops()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1 2 2DUP"));
        Assert.Equal(4, forth.Stack.Count);
        Assert.Equal(1L, (long)forth.Stack[0]);
        Assert.Equal(2L, (long)forth.Stack[1]);
        Assert.Equal(1L, (long)forth.Stack[2]);
        Assert.Equal(2L, (long)forth.Stack[3]);

        Assert.True(await forth.EvalAsync("2SWAP"));
        Assert.Equal(4, forth.Stack.Count);
        Assert.Equal(1L, (long)forth.Stack[0]);
        Assert.Equal(2L, (long)forth.Stack[1]);
        Assert.Equal(1L, (long)forth.Stack[2]);
        Assert.Equal(2L, (long)forth.Stack[3]);
    }

    /// <summary>
    /// Intention: Ensure extended comparisons and min/max behave per standard truth values (all bits set for true).
    /// Expected: 0= 0<> <> <= >= MIN MAX have correct results for representative inputs.
    /// </summary>
    [Fact] 
    public async Task Comparisons_Extended()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("0 0="));
        Assert.Equal(1L, (long)forth.Stack[^1]);
        Assert.True(await forth.EvalAsync("1 0<>"));
        Assert.Equal(1L, (long)forth.Stack[^1]);
        Assert.True(await forth.EvalAsync("2 3 <>"));
        Assert.Equal(1L, (long)forth.Stack[^1]);
        Assert.True(await forth.EvalAsync("2 2 <>"));
        Assert.Equal(0L, (long)forth.Stack[^1]);
        Assert.True(await forth.EvalAsync("2 3 <="));
        Assert.Equal(1L, (long)forth.Stack[^1]);
        Assert.True(await forth.EvalAsync("3 2 >="));
        Assert.Equal(1L, (long)forth.Stack[^1]);
        Assert.True(await forth.EvalAsync("2 5 MIN"));
        Assert.Equal(2L, (long)forth.Stack[^1]);
        Assert.True(await forth.EvalAsync("2 5 MAX"));
        Assert.Equal(5L, (long)forth.Stack[^1]);
    }

    /// <summary>
    /// Intention: Validate remainder and quotient variants return results matching ANS Forth definitions.
    /// Expected: 7 3 /MOD -> 1 2 ; 7 3 MOD -> 1 etc.
    /// </summary>
    [Fact] 
    public async Task DivMod_Variants()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("7 3 /MOD"));
        Assert.Equal(2L, (long)forth.Stack[^1]); // quotient
        Assert.Equal(1L, (long)forth.Stack[^2]); // remainder below it

        Assert.True(await forth.EvalAsync("7 3 MOD"));
        Assert.Equal(1L, (long)forth.Stack[^1]);
    }
}
