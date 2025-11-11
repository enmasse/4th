using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.Math;

public class ExtendedMathTests
{
    /// <summary>
    /// Intention: Provide coverage for core bitwise words to ensure correct logical operations on cells.
    /// Expected: 6 3 AND -> 2; 6 1 OR -> 7; 6 3 XOR -> 5; 0 INVERT -> -1 (two's complement).
    /// </summary>
    [Fact] 
    public void Bitwise_Basics()
    {
        var forth = new ForthInterpreter();
        Assert.True(forth.Interpret("6 3 AND"));
        Assert.Equal(2L, (long)forth.Stack[^1]);
        Assert.True(forth.Interpret("6 1 OR"));
        Assert.Equal(7L, (long)forth.Stack[^1]);
        Assert.True(forth.Interpret("6 3 XOR"));
        Assert.Equal(5L, (long)forth.Stack[^1]);
        Assert.True(forth.Interpret("0 INVERT"));
        Assert.Equal(-1L, (long)forth.Stack[^1]);
    }

    /// <summary>
    /// Intention: Validate arithmetic shifts with LSHIFT and RSHIFT on unsigned semantics per standard.
    /// Expected: 1 3 LSHIFT -> 8; 8 2 RSHIFT -> 2.
    /// </summary>
    [Fact] 
    public void Bitwise_Shifts()
    {
        var forth = new ForthInterpreter();
        Assert.True(forth.Interpret("1 3 LSHIFT"));
        Assert.Equal(8L, (long)forth.Stack[^1]);
        Assert.True(forth.Interpret("8 2 RSHIFT"));
        Assert.Equal(2L, (long)forth.Stack[^1]);
    }

    /// <summary>
    /// Intention: Exercise double-cell stack ops to ensure paired-cell ordering is preserved.
    /// Expected: 1 2 2DUP -> 1 2 1 2; then 2SWAP exchanges pairs.
    /// </summary>
    [Fact] 
    public void DoubleCell_Ops()
    {
        var forth = new ForthInterpreter();
        Assert.True(forth.Interpret("1 2 2DUP"));
        Assert.Equal(4, forth.Stack.Count);
        Assert.Equal(1L, (long)forth.Stack[0]);
        Assert.Equal(2L, (long)forth.Stack[1]);
        Assert.Equal(1L, (long)forth.Stack[2]);
        Assert.Equal(2L, (long)forth.Stack[3]);

        Assert.True(forth.Interpret("2SWAP"));
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
    [Fact(Skip = "Comparison variants not implemented yet")] 
    public void Comparisons_Extended()
    {
        var forth = new ForthInterpreter();
        // 0= 0<> <> <= >= MIN MAX
    }

    /// <summary>
    /// Intention: Validate remainder and quotient variants return results matching ANS Forth definitions.
    /// Expected: 7 3 /MOD -> 1 2 ; 7 3 MOD -> 1 etc.
    /// </summary>
    [Fact(Skip = "Division variants not implemented yet")] 
    public void DivMod_Variants()
    {
        var forth = new ForthInterpreter();
        // 7 3 /MOD -> 1 2 ; 7 3 MOD -> 1
    }
}
