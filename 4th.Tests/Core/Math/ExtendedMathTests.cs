using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.Math;

public class ExtendedMathTests
{
    /// <summary>
    /// Intention: Provide coverage for core bitwise words to ensure correct logical operations on cells.
    /// Expected: 6 3 AND -> 2; 6 1 OR -> 7; 6 3 XOR -> 5; 0 INVERT -> -1 (two's complement).
    /// </summary>
    [Fact(Skip = "Bitwise words AND OR XOR INVERT not implemented yet")] 
    public void Bitwise_Basics()
    {
        var forth = new ForthInterpreter();
        // 6 3 AND -> 2; 6 1 OR -> 7; 6 3 XOR -> 5; 0 INVERT -> -1
    }

    /// <summary>
    /// Intention: Validate arithmetic shifts with LSHIFT and RSHIFT on unsigned semantics per standard.
    /// Expected: 1 3 LSHIFT -> 8; 8 2 RSHIFT -> 2.
    /// </summary>
    [Fact(Skip = "Shift words LSHIFT RSHIFT not implemented yet")] 
    public void Bitwise_Shifts()
    {
        var forth = new ForthInterpreter();
        // 1 3 LSHIFT -> 8; 8 2 RSHIFT -> 2
    }

    /// <summary>
    /// Intention: Exercise double-cell stack ops to ensure paired-cell ordering is preserved.
    /// Expected: 1 2 2DUP -> 1 2 1 2;  1 2 2SWAP -> keeps pairs swapped.
    /// </summary>
    [Fact(Skip = "Double-cell ops not implemented yet")] 
    public void DoubleCell_Ops()
    {
        var forth = new ForthInterpreter();
        // 1 2 2DUP  -> 1 2 1 2;  1 2 2SWAP -> 1 2  (check ordering)
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
