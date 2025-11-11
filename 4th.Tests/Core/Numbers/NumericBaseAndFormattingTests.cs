using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.Numbers;

public class NumericBaseAndFormattingTests
{
    [Fact(Skip = "BASE/DECIMAL/HEX not implemented yet")] 
    public void BaseSwitching()
    {
        var forth = new ForthInterpreter();
        // DECIMAL 10  HEX  A  should parse in selected base
    }

    [Fact(Skip = "Pictured numeric output <# # #S HOLD SIGN #> not implemented yet")] 
    public void PicturedNumericOutput()
    {
        var forth = new ForthInterpreter();
        // <# 15 0 #S #> TYPE  -> "15"
    }

    [Fact(Skip = ">NUMBER not implemented yet")] 
    public void ToNumber_Parsing()
    {
        var forth = new ForthInterpreter();
        // S" 123" 0 0 >NUMBER -> 123 0 3
    }
}
