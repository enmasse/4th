using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.Numbers;

public class NumericBaseAndFormattingTests
{
    /// <summary>
    /// Intention: Confirm BASE changes affect number parsing and printing (DECIMAL vs HEX).
    /// Expected: Values entered after switching base interpret correctly (e.g., HEX A equals 10 decimal).
    /// </summary>
    [Fact]
    public void BaseSwitching()
    {
        var forth = new ForthInterpreter();
        Assert.True(forth.Interpret("DECIMAL 10"));
        Assert.Single(forth.Stack);
        Assert.Equal(10L, (long)forth.Stack[0]);

        Assert.True(forth.Interpret("HEX A"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(10L, (long)forth.Stack[^1]);

        Assert.True(forth.Interpret("DECIMAL 10"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(10L, (long)forth.Stack[^1]);
    }

    /// <summary>
    /// Intention: Exercise pictured numeric output words building a string representation manually.
    /// Expected: <# ... #> leaves address/len pair and TYPE prints number as expected.
    /// </summary>
    [Fact(Skip = "Pictured numeric output <# # #S HOLD SIGN #> not implemented yet")] 
    public void PicturedNumericOutput()
    {
        var forth = new ForthInterpreter();
        // <# 15 0 #S #> TYPE  -> "15"
    }

    /// <summary>
    /// Intention: Verify >NUMBER converts a string to its numeric value producing residual substring if any.
    /// Expected: S" 123" 0 0 >NUMBER yields 123 and zero remainder length.
    /// </summary>
    [Fact(Skip = ">NUMBER not implemented yet")] 
    public void ToNumber_Parsing()
    {
        var forth = new ForthInterpreter();
        // S" 123" 0 0 >NUMBER -> 123 0 3
    }
}
