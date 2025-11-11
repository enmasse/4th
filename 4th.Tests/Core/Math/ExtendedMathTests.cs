using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.Math;

public class ExtendedMathTests
{
    [Fact(Skip = "Bitwise words AND OR XOR INVERT not implemented yet")] 
    public void Bitwise_Basics()
    {
        var forth = new ForthInterpreter();
        // 6 3 AND -> 2; 6 1 OR -> 7; 6 3 XOR -> 5; 0 INVERT -> -1
    }

    [Fact(Skip = "Shift words LSHIFT RSHIFT not implemented yet")] 
    public void Bitwise_Shifts()
    {
        var forth = new ForthInterpreter();
        // 1 3 LSHIFT -> 8; 8 2 RSHIFT -> 2
    }

    [Fact(Skip = "Double-cell ops not implemented yet")] 
    public void DoubleCell_Ops()
    {
        var forth = new ForthInterpreter();
        // 1 2 2DUP  -> 1 2 1 2;  1 2 2SWAP -> 1 2  (check ordering)
    }

    [Fact(Skip = "Comparison variants not implemented yet")] 
    public void Comparisons_Extended()
    {
        var forth = new ForthInterpreter();
        // 0= 0<> <> <= >= MIN MAX
    }

    [Fact(Skip = "Division variants not implemented yet")] 
    public void DivMod_Variants()
    {
        var forth = new ForthInterpreter();
        // 7 3 /MOD -> 1 2 ; 7 3 MOD -> 1
    }
}
