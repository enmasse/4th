using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

/// <summary>
/// Regression tests for floating-point primitives, especially >FLOAT and related recent changes.
/// These tests ensure critical floating-point functionality continues to work correctly.
/// </summary>
public class FloatingPointRegressionTests
{
    #region >FLOAT Tests (String to Float Conversion)

    [Fact]
    public async Task ToFloat_ValidPositiveNumber()
    {
        var forth = new ForthInterpreter();
        // Push string "3.14" and its length
        Assert.True(await forth.EvalAsync("S\" 3.14\" >FLOAT"));
        
        // Should leave: result true
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[^1]); // true flag
        Assert.Equal(3.14, (double)forth.Stack[^2], 5);
    }

    [Fact]
    public async Task ToFloat_ValidNegativeNumber()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("S\" -2.5\" >FLOAT"));
        
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[^1]); // true
        Assert.Equal(-2.5, (double)forth.Stack[^2], 5);
    }

    [Fact]
    public async Task ToFloat_ScientificNotation()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("S\" 1.5e2\" >FLOAT"));
        
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[^1]); // true
        Assert.Equal(150.0, (double)forth.Stack[^2], 5);
    }

    [Fact]
    public async Task ToFloat_NegativeExponent()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("S\" 2.5e-1\" >FLOAT"));
        
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[^1]); // true
        Assert.Equal(0.25, (double)forth.Stack[^2], 5);
    }

    [Fact]
    public async Task ToFloat_IntegerString()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("S\" 42\" >FLOAT"));
        
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[^1]); // true
        Assert.Equal(42.0, (double)forth.Stack[^2], 5);
    }

    [Fact]
    public async Task ToFloat_Zero()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("S\" 0\" >FLOAT"));
        
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[^1]); // true
        Assert.Equal(0.0, (double)forth.Stack[^2], 5);
    }

    [Fact]
    public async Task ToFloat_DecimalPoint()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("S\" 0.5\" >FLOAT"));
        
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[^1]); // true
        Assert.Equal(0.5, (double)forth.Stack[^2], 5);
    }

    [Fact]
    public async Task ToFloat_InvalidString_ReturnsFalse()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("S\" abc\" >FLOAT"));
        
        // Should leave: c-addr u false
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[^1]); // false flag
        Assert.Equal(3L, (long)forth.Stack[^2]); // length
        Assert.IsType<long>(forth.Stack[^3]); // address
    }

    [Fact]
    public async Task ToFloat_EmptyString()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("S\" \" >FLOAT"));
        
        // Empty string should convert to 0.0 and return true
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[^1]); // true
        Assert.Equal(0.0, (double)forth.Stack[^2], 5);
    }

    [Fact]
    public async Task ToFloat_JustDecimalPoint_ReturnsFalse()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("S\" .\" >FLOAT"));
        
        // Just a dot should fail
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[^1]); // false
    }

    [Fact]
    public async Task ToFloat_WithLeadingWhitespace_ReturnsFalse()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("S\"  3.14\" >FLOAT"));
        
        // Forth >FLOAT should reject leading whitespace
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[^1]); // false
    }

    [Fact]
    public async Task ToFloat_WithTrailingWhitespace_ReturnsFalse()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("S\" 3.14 \" >FLOAT"));
        
        // Forth >FLOAT should reject trailing whitespace
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[^1]); // false
    }

    [Fact]
    public async Task ToFloat_VeryLargeNumber()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("S\" 1.7976931348623157e308\" >FLOAT"));
        
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[^1]); // true
        Assert.True((double)forth.Stack[^2] > 1e308);
    }

    [Fact]
    public async Task ToFloat_VerySmallNumber()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("S\" 2.2250738585072014e-308\" >FLOAT"));
        
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[^1]); // true
        Assert.True((double)forth.Stack[^2] < 1e-307);
    }

    #endregion

    #region FOVER, FDROP, FDEPTH, FLOATS Tests

    [Fact]
    public async Task FOVER_CopiesToTop()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1.5d 2.5d FOVER"));
        
        // Should have: 1.5 2.5 1.5
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(1.5, (double)forth.Stack[0], 5);
        Assert.Equal(2.5, (double)forth.Stack[1], 5);
        Assert.Equal(1.5, (double)forth.Stack[2], 5);
    }

    [Fact]
    public async Task FDROP_DropsTopItem()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1.5d 2.5d FDROP"));
        
        Assert.Single(forth.Stack);
        Assert.Equal(1.5, (double)forth.Stack[0], 5);
    }

    [Fact]
    public async Task FDEPTH_CountsFloatingItems()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1.5d 2.5d 3.5d FDEPTH"));
        
        // Should push count of double items
        Assert.Equal(4, forth.Stack.Count);
        Assert.Equal(3L, (long)forth.Stack[^1]);
    }

    [Fact]
    public async Task FDEPTH_WithMixedStack()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("42 1.5d 99 2.5d FDEPTH"));
        
        // Should count only the two doubles
        Assert.Equal(5, forth.Stack.Count);
        Assert.Equal(2L, (long)forth.Stack[^1]);
    }

    [Fact]
    public async Task FLOATS_ScalesSize()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("5 FLOATS"));
        
        // In this implementation, FLOATS returns n unchanged
        Assert.Single(forth.Stack);
        Assert.Equal(5L, (long)forth.Stack[0]);
    }

    [Fact]
    public async Task FDUP_DuplicatesTopItem()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("3.14d FDUP"));
        
        // Should have two copies of 3.14
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(3.14, (double)forth.Stack[0], 5);
        Assert.Equal(3.14, (double)forth.Stack[1], 5);
    }

    [Fact]
    public async Task FDUP_WorksWithNegative()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("-2.5d FDUP"));
        
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-2.5, (double)forth.Stack[0], 5);
        Assert.Equal(-2.5, (double)forth.Stack[1], 5);
    }

    [Fact]
    public async Task FDUP_WorksWithZero()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("0.0d FDUP"));
        
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(0.0, (double)forth.Stack[0], 5);
        Assert.Equal(0.0, (double)forth.Stack[1], 5);
    }

    #endregion

    #region D>F and F>D Tests

    [Fact]
    public async Task DToF_PositiveDouble()
    {
        var forth = new ForthInterpreter();
        // Push double-cell number (lo hi)
        Assert.True(await forth.EvalAsync("12345 0 D>F"));
        
        Assert.Single(forth.Stack);
        Assert.Equal(12345.0, (double)forth.Stack[0], 5);
    }

    [Fact]
    public async Task DToF_NegativeDouble()
    {
        var forth = new ForthInterpreter();
        // Push negative double-cell number
        Assert.True(await forth.EvalAsync("-500 -1 D>F"));
        
        Assert.Single(forth.Stack);
        Assert.Equal(-500.0, (double)forth.Stack[0], 5);
    }

    [Fact]
    public async Task FToD_PositiveFloat()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("123.7d F>D"));
        
        // Should push lo hi
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(123L, (long)forth.Stack[0]); // lo (truncated)
        Assert.Equal(0L, (long)forth.Stack[1]); // hi (sign extension)
    }

    [Fact]
    public async Task FToD_NegativeFloat()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("-456.2d F>D"));
        
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-456L, (long)forth.Stack[0]); // lo
        Assert.Equal(-1L, (long)forth.Stack[1]); // hi (sign extension)
    }

    [Fact]
    public async Task FToD_Zero()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("0.0d F>D"));
        
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[0]);
        Assert.Equal(0L, (long)forth.Stack[1]);
    }

    #endregion

    #region SF! and SF@ Tests (Single Precision)

    [Fact]
    public async Task SF_StoreAndFetch()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("VARIABLE SFVAR"));
        Assert.True(await forth.EvalAsync("3.14159d SFVAR SF!"));
        Assert.True(await forth.EvalAsync("SFVAR SF@"));
        
        // Should fetch as double (promoted from single)
        Assert.Single(forth.Stack);
        // Single precision will lose some accuracy
        Assert.Equal(3.14159, (double)forth.Stack[0], 4);
    }

    [Fact]
    public async Task SF_NegativeValue()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("VARIABLE SFVAR2"));
        Assert.True(await forth.EvalAsync("-2.5d SFVAR2 SF!"));
        Assert.True(await forth.EvalAsync("SFVAR2 SF@"));
        
        Assert.Single(forth.Stack);
        Assert.Equal(-2.5, (double)forth.Stack[0], 4);
    }

    #endregion

    #region DF! and DF@ Tests (Double Precision)

    [Fact]
    public async Task DF_StoreAndFetch()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("VARIABLE DFVAR"));
        Assert.True(await forth.EvalAsync("3.141592653589793d DFVAR DF!"));
        Assert.True(await forth.EvalAsync("DFVAR DF@"));
        
        Assert.Single(forth.Stack);
        Assert.Equal(3.141592653589793, (double)forth.Stack[0], 15);
    }

    [Fact]
    public async Task DF_NegativeValue()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("VARIABLE DFVAR2"));
        Assert.True(await forth.EvalAsync("-123.456789d DFVAR2 DF!"));
        Assert.True(await forth.EvalAsync("DFVAR2 DF@"));
        
        Assert.Single(forth.Stack);
        Assert.Equal(-123.456789, (double)forth.Stack[0], 15);
    }

    #endregion

    #region F! and F@ Regression Tests

    [Fact]
    public async Task F_StoreAndFetch_WithFVARIABLE()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("FVARIABLE MYVAR"));
        Assert.True(await forth.EvalAsync("2.718281828459045d MYVAR F!"));
        Assert.True(await forth.EvalAsync("MYVAR F@"));
        
        Assert.Single(forth.Stack);
        Assert.Equal(2.718281828459045, (double)forth.Stack[0], 15);
    }

    [Fact]
    public async Task F_MultipleStoresAndFetches()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("FVARIABLE VAR1"));
        Assert.True(await forth.EvalAsync("FVARIABLE VAR2"));
        
        Assert.True(await forth.EvalAsync("1.11d VAR1 F!"));
        Assert.True(await forth.EvalAsync("2.22d VAR2 F!"));
        
        Assert.True(await forth.EvalAsync("VAR1 F@ VAR2 F@"));
        
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(1.11, (double)forth.Stack[0], 5);
        Assert.Equal(2.22, (double)forth.Stack[1], 5);
    }

    #endregion

    #region Division by Zero Protection (IEEE 754 Semantics)

    [Fact]
    public async Task FSlash_DivideByZero_ProducesInfinity()
    {
        var forth = new ForthInterpreter();
        
        // IEEE 754: division by zero produces Infinity, not exception
        Assert.True(await forth.EvalAsync("10.0d 0.0d F/"));
        
        Assert.Single(forth.Stack);
        Assert.True(double.IsPositiveInfinity((double)forth.Stack[0]));
    }

    [Fact]
    public async Task FSlash_VerySmallDivisor_DoesNotThrow()
    {
        var forth = new ForthInterpreter();
        // Very small but non-zero divisor should work
        Assert.True(await forth.EvalAsync("10.0d 0.0000001d F/"));
        
        Assert.Single(forth.Stack);
        Assert.True((double)forth.Stack[0] > 1e7);
    }

    #endregion

    #region ToDoubleFromObj Helper Coverage

    [Fact]
    public async Task ToDoubleFromObj_HandlesIntegerInput()
    {
        var forth = new ForthInterpreter();
        // Integer should be convertible to double in F+ operation
        Assert.True(await forth.EvalAsync("42 1.5d F+"));
        
        Assert.Single(forth.Stack);
        Assert.Equal(43.5, (double)forth.Stack[0], 5);
    }

    [Fact]
    public async Task ToDoubleFromObj_HandlesLongInput()
    {
        var forth = new ForthInterpreter();
        // Large integer should convert to double
        Assert.True(await forth.EvalAsync("1000000 2.5d F*"));
        
        Assert.Single(forth.Stack);
        Assert.Equal(2500000.0, (double)forth.Stack[0], 5);
    }

    [Fact]
    public async Task ToDoubleFromObj_HandlesMixedTypes()
    {
        var forth = new ForthInterpreter();
        // Test mixing int and double in operations
        Assert.True(await forth.EvalAsync("10 5.0d F- 2 F*"));
        
        Assert.Single(forth.Stack);
        Assert.Equal(10.0, (double)forth.Stack[0], 5);
    }

    #endregion

    #region Edge Cases and Boundary Conditions

    [Fact]
    public async Task FloatingPoint_DivideByZero_ProducesNaN()
    {
        var forth = new ForthInterpreter();
        // IEEE 754: 0.0 / 0.0 produces NaN
        Assert.True(await forth.EvalAsync("0.0d 0.0d F/"));
        
        Assert.Single(forth.Stack);
        Assert.True(double.IsNaN((double)forth.Stack[0]));
    }

    [Fact]
    public async Task FloatingPoint_DivideByZero_ProducesPositiveInfinity()
    {
        var forth = new ForthInterpreter();
        // IEEE 754: 1.0 / 0.0 produces +infinity
        Assert.True(await forth.EvalAsync("1.0d 0.0d F/"));
        
        Assert.Single(forth.Stack);
        Assert.True(double.IsPositiveInfinity((double)forth.Stack[0]));
    }

    [Fact]
    public async Task FloatingPoint_DivideByZero_ProducesNegativeInfinity()
    {
        var forth = new ForthInterpreter();
        // IEEE 754: -1.0 / 0.0 produces -infinity
        Assert.True(await forth.EvalAsync("-1.0d 0.0d F/"));
        
        Assert.Single(forth.Stack);
        Assert.True(double.IsNegativeInfinity((double)forth.Stack[0]));
    }

    [Fact]
    public async Task FloatingPoint_UnderflowToZero()
    {
        var forth = new ForthInterpreter();
        // Very small number squared should underflow gracefully
        Assert.True(await forth.EvalAsync("1.0e-200d 1.0e-200d F*"));
        
        Assert.Single(forth.Stack);
        var result = (double)forth.Stack[0];
        // Should either be zero or a very small denormalized number
        Assert.True(result == 0.0 || System.Math.Abs(result) < 1.0e-300);
    }

    #endregion

    #region Comparison Operations Regression

    [Fact]
    public async Task F_Comparisons_WithZero()
    {
        var forth = new ForthInterpreter();
        
        // Test F0= with actual zero
        Assert.True(await forth.EvalAsync("0.0d F0="));
        Assert.Equal(-1L, (long)forth.Stack[^1]); // true
        
        // Test F0= with non-zero
        Assert.True(await forth.EvalAsync("0.1d F0="));
        Assert.Equal(0L, (long)forth.Stack[^1]); // false
        
        // Test F0< with negative
        Assert.True(await forth.EvalAsync("-0.1d F0<"));
        Assert.Equal(-1L, (long)forth.Stack[^1]); // true
        
        // Test F0< with positive
        Assert.True(await forth.EvalAsync("0.1d F0<"));
        Assert.Equal(0L, (long)forth.Stack[^1]); // false
    }

    [Fact]
    public async Task F_Equality_WithTolerance()
    {
        var forth = new ForthInterpreter();
        
        // Exact equality
        Assert.True(await forth.EvalAsync("3.14d 3.14d F="));
        Assert.Equal(-1L, (long)forth.Stack[^1]); // true
        
        // Very close but not equal (due to floating point precision)
        Assert.True(await forth.EvalAsync("3.14d 3.140001d F="));
        Assert.Equal(0L, (long)forth.Stack[^1]); // false
    }

    #endregion

    #region Math Function Boundary Tests

    [Fact]
    public async Task FSQRT_NegativeInput_HandlesGracefully()
    {
        var forth = new ForthInterpreter();
        // sqrt of negative should produce NaN
        Assert.True(await forth.EvalAsync("-1.0d FSQRT"));
        
        Assert.Single(forth.Stack);
        Assert.True(double.IsNaN((double)forth.Stack[0]));
    }

    [Fact]
    public async Task FLOG_NegativeInput_HandlesGracefully()
    {
        var forth = new ForthInterpreter();
        // log of negative should produce NaN
        Assert.True(await forth.EvalAsync("-1.0d FLOG"));
        
        Assert.Single(forth.Stack);
        Assert.True(double.IsNaN((double)forth.Stack[0]));
    }

    [Fact]
    public async Task FACOS_OutOfRange_HandlesGracefully()
    {
        var forth = new ForthInterpreter();
        // acos of value > 1 should produce NaN
        Assert.True(await forth.EvalAsync("2.0d FACOS"));
        
        Assert.Single(forth.Stack);
        Assert.True(double.IsNaN((double)forth.Stack[0]));
    }

    [Fact]
    public async Task FASIN_OutOfRange_HandlesGracefully()
    {
        var forth = new ForthInterpreter();
        // asin of value > 1 should produce NaN
        Assert.True(await forth.EvalAsync("-2.0d FASIN"));
        
        Assert.Single(forth.Stack);
        Assert.True(double.IsNaN((double)forth.Stack[0]));
    }

    #endregion

    #region Stack Effect Verification

    [Fact]
    public async Task AllFloatingOps_PreserveStackIntegrity()
    {
        var forth = new ForthInterpreter();
        
        // Push a marker value
        Assert.True(await forth.EvalAsync("999"));
        
        // Perform various floating operations
        Assert.True(await forth.EvalAsync("3.14d 2.0d F+ FDROP"));
        
        // Marker should still be there
        Assert.Single(forth.Stack);
        Assert.Equal(999L, (long)forth.Stack[0]);
    }

    #endregion
}
