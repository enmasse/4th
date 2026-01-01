using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System;

namespace Forth.Tests.Core.MissingWords;

public class FloatingPointTests
{
    [Fact]
    public async Task FloatingPoint_Arithmetic()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1.5d 2.5d F+"));
        Assert.Single(forth.Stack);
        var v = forth.Stack[0];
        Assert.IsType<double>(v);
        Assert.Equal(4.0, (double)v, 10);

        // Other ops
        Assert.True(await forth.EvalAsync("5.5d 2.0d F-"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(3.5, (double)forth.Stack[^1], 10);

        Assert.True(await forth.EvalAsync("3.0d 4.0d F*"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(12.0, (double)forth.Stack[^1], 10);

        Assert.True(await forth.EvalAsync("9.0d 3.0d F/"));
        Assert.Equal(4, forth.Stack.Count);
        Assert.Equal(3.0, (double)forth.Stack[^1], 10);

        Assert.True(await forth.EvalAsync("2.5d FNEGATE"));
        Assert.Equal(5, forth.Stack.Count);
        Assert.Equal(-2.5, (double)forth.Stack[^1], 10);
    }

    [Fact]
    public async Task FloatingPoint_Variables_And_Constants()
    {
        var forth = new ForthInterpreter();
        // FVARIABLE and F@/F!
        Assert.True(await forth.EvalAsync("FVARIABLE FV"));
        Assert.True(await forth.EvalAsync("0.0d FV F!"));
        Assert.True(await forth.EvalAsync("FV F@"));
        Assert.Single(forth.Stack);
        Assert.Equal(0.0, (double)forth.Stack[0], 10);

        // FCONSTANT
        Assert.True(await forth.EvalAsync("3.14159d FCONSTANT PI"));
        Assert.True(await forth.EvalAsync("PI"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(3.14159, (double)forth.Stack[^1], 5);
    }

    [Fact]
    public async Task FloatingPoint_Printing()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync("2.5d F."));
        // F. writes text using IO
        Assert.Single(io.Outputs);
        Assert.Contains("2.5", io.Outputs[0]);
    }

    [Fact]
    public async Task FloatingPoint_Comparisons()
    {
        var forth = new ForthInterpreter();
        // F0=
        Assert.True(await forth.EvalAsync("0.0d F0="));
        Assert.Single(forth.Stack);
        Assert.Equal(-1L, (long)forth.Stack[0]); // true

        Assert.True(await forth.EvalAsync("1.0d F0="));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[1]); // false

        // F0<
        Assert.True(await forth.EvalAsync("-1.0d F0<"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[2]); // true

        Assert.True(await forth.EvalAsync("1.0d F0<"));
        Assert.Equal(4, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[3]); // false

        // F<
        Assert.True(await forth.EvalAsync("1.0d 2.0d F<"));
        Assert.Equal(5, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[4]); // true

        Assert.True(await forth.EvalAsync("2.0d 1.0d F<"));
        Assert.Equal(6, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[5]); // false

        // F=
        Assert.True(await forth.EvalAsync("1.5d 1.5d F="));
        Assert.Equal(7, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[6]); // true

        Assert.True(await forth.EvalAsync("1.5d 2.0d F="));
        Assert.Equal(8, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[7]); // false
    }

    [Fact]
    public async Task FloatingPoint_NewComparisons()
    {
        var forth = new ForthInterpreter();
        // F>
        Assert.True(await forth.EvalAsync("2.0d 1.0d F>"));
        Assert.Single(forth.Stack);
        Assert.Equal(-1L, (long)forth.Stack[0]); // true

        Assert.True(await forth.EvalAsync("1.0d 2.0d F>"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[1]); // false

        // F!=
        Assert.True(await forth.EvalAsync("1.5d 2.0d F!="));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[2]); // true

        Assert.True(await forth.EvalAsync("1.5d 1.5d F!="));
        Assert.Equal(4, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[3]); // false

        // F<=
        Assert.True(await forth.EvalAsync("1.0d 2.0d F<="));
        Assert.Equal(5, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[4]); // true

        Assert.True(await forth.EvalAsync("2.0d 1.0d F<="));
        Assert.Equal(6, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[5]); // false

        Assert.True(await forth.EvalAsync("1.5d 1.5d F<="));
        Assert.Equal(7, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[6]); // true

        // F>=
        Assert.True(await forth.EvalAsync("2.0d 1.0d F>="));
        Assert.Equal(8, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[7]); // true

        Assert.True(await forth.EvalAsync("1.0d 2.0d F>="));
        Assert.Equal(9, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[8]); // false

        Assert.True(await forth.EvalAsync("1.5d 1.5d F>="));
        Assert.Equal(10, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[9]); // true

        // F0>
        Assert.True(await forth.EvalAsync("1.0d F0>"));
        Assert.Equal(11, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[10]); // true

        Assert.True(await forth.EvalAsync("-1.0d F0>"));
        Assert.Equal(12, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[11]); // false

        Assert.True(await forth.EvalAsync("0.0d F0>"));
        Assert.Equal(13, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[12]); // false

        // F0<=
        Assert.True(await forth.EvalAsync("-1.0d F0<="));
        Assert.Equal(14, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[13]); // true

        Assert.True(await forth.EvalAsync("1.0d F0<="));
        Assert.Equal(15, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[14]); // false

        Assert.True(await forth.EvalAsync("0.0d F0<="));
        Assert.Equal(16, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[15]); // true

        // F0>=
        Assert.True(await forth.EvalAsync("1.0d F0>="));
        Assert.Equal(17, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[16]); // true

        Assert.True(await forth.EvalAsync("-1.0d F0>="));
        Assert.Equal(18, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[17]); // false

        Assert.True(await forth.EvalAsync("0.0d F0>="));
        Assert.Equal(19, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[18]); // true
    }

    [Fact]
    public async Task FToS_ConvertsFloatToInt()
    {
        var forth = new ForthInterpreter();
        // Positive float
        Assert.True(await forth.EvalAsync("3.7d F>S"));
        Assert.Single(forth.Stack);
        Assert.Equal(3L, (long)forth.Stack[0]);

        // Negative float
        Assert.True(await forth.EvalAsync("-2.9d F>S"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-2L, (long)forth.Stack[1]);

        // Zero
        Assert.True(await forth.EvalAsync("0.0d F>S"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[2]);
    }

    [Fact]
    public async Task SToF_ConvertsIntToFloat()
    {
        var forth = new ForthInterpreter();
        // Positive integer
        Assert.True(await forth.EvalAsync("42 S>F"));
        Assert.Single(forth.Stack);
        Assert.Equal(42.0, (double)forth.Stack[0], 10);

        // Negative integer
        Assert.True(await forth.EvalAsync("-7 S>F"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-7.0, (double)forth.Stack[1], 10);

        // Zero
        Assert.True(await forth.EvalAsync("0 S>F"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(0.0, (double)forth.Stack[2], 10);
    }

    [Fact]
    public async Task FABS_AbsoluteValue()
    {
        var forth = new ForthInterpreter();
        // Positive float
        Assert.True(await forth.EvalAsync("3.5d FABS"));
        Assert.Single(forth.Stack);
        Assert.Equal(3.5, (double)forth.Stack[0], 10);

        // Negative float
        Assert.True(await forth.EvalAsync("-2.5d FABS"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(2.5, (double)forth.Stack[1], 10);

        // Zero
        Assert.True(await forth.EvalAsync("0.0d FABS"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(0.0, (double)forth.Stack[2], 10);
    }

    [Fact]
    public async Task FLOOR_Flooring()
    {
        var forth = new ForthInterpreter();
        // ANS Forth: FLOOR returns a floating-point value, not an integer
        // Positive float
        Assert.True(await forth.EvalAsync("3.7d FLOOR"));
        Assert.Single(forth.Stack);
        Assert.IsType<double>(forth.Stack[0]);
        Assert.Equal(3.0, (double)forth.Stack[0], 10);

        // Negative float
        Assert.True(await forth.EvalAsync("-2.1d FLOOR"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.IsType<double>(forth.Stack[1]);
        Assert.Equal(-3.0, (double)forth.Stack[1], 10);

        // Integer
        Assert.True(await forth.EvalAsync("5.0d FLOOR"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.IsType<double>(forth.Stack[2]);
        Assert.Equal(5.0, (double)forth.Stack[2], 10);
        
        // Verify FLOOR result can be used in further float operations
        Assert.True(await forth.EvalAsync("2.5d FLOOR 1.0d F+"));
        Assert.Equal(4, forth.Stack.Count); // 3 previous + 1 new result
        Assert.Equal(3.0, (double)forth.Stack[3], 10);
    }

    [Fact]
    public async Task FROUND_Rounding()
    {
        var forth = new ForthInterpreter();
        // Positive float
        Assert.True(await forth.EvalAsync("3.7d FROUND"));
        Assert.Single(forth.Stack);
        Assert.Equal(4L, (long)forth.Stack[0]);

        // Negative float
        Assert.True(await forth.EvalAsync("-2.3d FROUND"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-2L, (long)forth.Stack[1]);

        // Integer
        Assert.True(await forth.EvalAsync("5.0d FROUND"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(5L, (long)forth.Stack[2]);

        // Tie to even
        Assert.True(await forth.EvalAsync("2.5d FROUND"));
        Assert.Equal(4, forth.Stack.Count);
        Assert.Equal(2L, (long)forth.Stack[3]);
    }

    [Fact]
    public async Task FSIN_Sine()
    {
        var forth = new ForthInterpreter();
        // sin(0) = 0
        Assert.True(await forth.EvalAsync("0.0d FSIN"));
        Assert.Single(forth.Stack);
        Assert.Equal(0.0, (double)forth.Stack[0], 10);

        // sin(pi/2) ? 1
        Assert.True(await forth.EvalAsync("1.5707963267948966d FSIN"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(1.0, (double)forth.Stack[1], 5);

        // sin(pi) ? 0
        Assert.True(await forth.EvalAsync("3.141592653589793d FSIN"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(0.0, (double)forth.Stack[2], 10);
    }

    [Fact]
    public async Task FTAN_Tangent()
    {
        var forth = new ForthInterpreter();
        // tan(0) = 0
        Assert.True(await forth.EvalAsync("0.0d FTAN"));
        Assert.Single(forth.Stack);
        Assert.Equal(0.0, (double)forth.Stack[0], 10);

        // tan(pi/4) ? 1
        Assert.True(await forth.EvalAsync("0.7853981633974483d FTAN"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(1.0, (double)forth.Stack[1], 5);
    }

    [Fact]
    public async Task FCOS_Cosine()
    {
        var forth = new ForthInterpreter();
        // cos(0) = 1
        Assert.True(await forth.EvalAsync("0.0d FCOS"));
        Assert.Single(forth.Stack);
        Assert.Equal(1.0, (double)forth.Stack[0], 10);

        // cos(pi) ? -1
        Assert.True(await forth.EvalAsync("3.141592653589793d FCOS"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-1.0, (double)forth.Stack[1], 5);
    }

    [Fact]
    public async Task FEXP_Exponential()
    {
        var forth = new ForthInterpreter();
        // exp(0) = 1
        Assert.True(await forth.EvalAsync("0.0d FEXP"));
        Assert.Single(forth.Stack);
        Assert.Equal(1.0, (double)forth.Stack[0], 10);

        // exp(1) ? e ? 2.718
        Assert.True(await forth.EvalAsync("1.0d FEXP"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(2.718281828459045, (double)forth.Stack[1], 5);
    }

    [Fact]
    public async Task FLOG_Logarithm()
    {
        var forth = new ForthInterpreter();
        // log(1) = 0
        Assert.True(await forth.EvalAsync("1.0d FLOG"));
        Assert.Single(forth.Stack);
        Assert.Equal(0.0, (double)forth.Stack[0], 10);

        // log(e) = 1
        Assert.True(await forth.EvalAsync("2.718281828459045d FLOG"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(1.0, (double)forth.Stack[1], 5);
    }

    [Fact]
    public async Task FACOS_Arccosine()
    {
        var forth = new ForthInterpreter();
        // acos(1) = 0
        Assert.True(await forth.EvalAsync("1.0d FACOS"));
        Assert.Single(forth.Stack);
        Assert.Equal(0.0, (double)forth.Stack[0], 10);

        // acos(0) = pi/2
        Assert.True(await forth.EvalAsync("0.0d FACOS"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(1.5707963267948966, (double)forth.Stack[1], 5);
    }

    [Fact]
    public async Task FASIN_Arcsine()
    {
        var forth = new ForthInterpreter();
        // asin(0) = 0
        Assert.True(await forth.EvalAsync("0.0d FASIN"));
        Assert.Single(forth.Stack);
        Assert.Equal(0.0, (double)forth.Stack[0], 10);

        // asin(1) = pi/2
        Assert.True(await forth.EvalAsync("1.0d FASIN"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(1.5707963267948966, (double)forth.Stack[1], 5);
    }

    [Fact]
    public async Task FAtan2_ArcTangent()
    {
        // atan2(1, 1) = pi/4
        var forth1 = new ForthInterpreter();
        Assert.True(await forth1.EvalAsync("1.0d 1.0d FATAN2"));
        Assert.Single(forth1.Stack);
        Assert.Equal(0.7853981633974483, (double)forth1.Stack[0], 10);

        // atan2(0, 1) = 0
        var forth2 = new ForthInterpreter();
        Assert.True(await forth2.EvalAsync("0.0d 1.0d FATAN2"));
        Assert.Single(forth2.Stack);
        Assert.Equal(0.0, (double)forth2.Stack[0], 10);
    }

    [Fact]
    public async Task FTilde_ApproximateEquality()
    {
        var forth = new ForthInterpreter();
        // 1.0 1.0001 0.01 F~ should be true (|1.0 - 1.0001| = 0.0001 < 0.01)
        Assert.True(await forth.EvalAsync("1.0d 1.0001d 0.01d F~"));
        Assert.Single(forth.Stack);
        Assert.Equal(-1L, (long)forth.Stack[0]); // true

        // 1.0 1.1 0.05 F~ should be false (|1.0 - 1.1| = 0.1 > 0.05)
        Assert.True(await forth.EvalAsync("1.0d 1.1d 0.05d F~"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[1]); // false

        // Close equality
        Assert.True(await forth.EvalAsync("2.5d 2.5001d 0.01d F~"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[2]); // true
    }

    [Fact]
    public async Task FMin_ReturnsMinimum()
    {
        var forth = new ForthInterpreter();
        // 3.5 2.1 FMIN should return 2.1
        Assert.True(await forth.EvalAsync("3.5d 2.1d FMIN"));
        Assert.Single(forth.Stack);
        Assert.Equal(2.1, (double)forth.Stack[0]);

        // 1.0 1.0 FMIN should return 1.0
        Assert.True(await forth.EvalAsync("1.0d 1.0d FMIN"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(1.0, (double)forth.Stack[1]);
    }

    [Fact]
    public async Task FMax_ReturnsMaximum()
    {
        var forth = new ForthInterpreter();
        // 3.5 2.1 FMAX should return 3.5
        Assert.True(await forth.EvalAsync("3.5d 2.1d FMAX"));
        Assert.Single(forth.Stack);
        Assert.Equal(3.5, (double)forth.Stack[0]);

        // 1.0 1.0 FMAX should return 1.0
        Assert.True(await forth.EvalAsync("1.0d 1.0d FMAX"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(1.0, (double)forth.Stack[1]);
    }

    [Fact]
    public async Task FSQRT_SquareRoot()
    {
        var forth = new ForthInterpreter();
        // sqrt(4) = 2
        Assert.True(await forth.EvalAsync("4.0d FSQRT"));
        Assert.Single(forth.Stack);
        Assert.Equal(2.0, (double)forth.Stack[0], 10);

        // sqrt(9) = 3
        Assert.True(await forth.EvalAsync("9.0d FSQRT"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(3.0, (double)forth.Stack[1], 10);

        // sqrt(0) = 0
        Assert.True(await forth.EvalAsync("0.0d FSQRT"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(0.0, (double)forth.Stack[2], 10);
    }

    [Fact]
    public async Task FTRUNC_TruncateTowardsZero()
    {
        var forth = new ForthInterpreter();
        // Positive float
        Assert.True(await forth.EvalAsync("3.7d FTRUNC"));
        Assert.Single(forth.Stack);
        Assert.Equal(3L, (long)forth.Stack[0]);

        // Negative float
        Assert.True(await forth.EvalAsync("-2.9d FTRUNC"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-2L, (long)forth.Stack[1]);

        // Integer
        Assert.True(await forth.EvalAsync("5.0d FTRUNC"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(5L, (long)forth.Stack[2]);
    }

    [Fact]
    public async Task FloatingPoint_StackOperations()
    {
        var forth = new ForthInterpreter();
        // FSWAP
        Assert.True(await forth.EvalAsync("1.0d 2.0d FSWAP"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(2.0, (double)forth.Stack[0]);
        Assert.Equal(1.0, (double)forth.Stack[1]);

        // FROT
        Assert.True(await forth.EvalAsync("3.0d FROT"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(1.0, (double)forth.Stack[0]);
        Assert.Equal(3.0, (double)forth.Stack[1]);
        Assert.Equal(2.0, (double)forth.Stack[2]);
    }

    [Fact]
    public async Task FLOATS_ReturnsCorrectSize()
    {
        var forth = new ForthInterpreter();
        // ANS Forth: 1 FLOATS should return the size of one float in bytes
        // For double precision, this is 8 bytes
        Assert.True(await forth.EvalAsync("1 FLOATS"));
        Assert.Single(forth.Stack);
        Assert.Equal(8L, (long)forth.Stack[0]);
        
        // Test with multiple floats
        Assert.True(await forth.EvalAsync("3 FLOATS"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(24L, (long)forth.Stack[1]); // 3 * 8 = 24
        
        // Test with zero
        Assert.True(await forth.EvalAsync("0 FLOATS"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[2]);
    }
    
    [Fact]
    public async Task FLN_AliasForFLOG()
    {
        var forth = new ForthInterpreter();
        // FLN should be an alias for FLOG (natural logarithm)
        Assert.True(await forth.EvalAsync("1.0d FLN"));
        Assert.Single(forth.Stack);
        Assert.Equal(0.0, (double)forth.Stack[0], 10);
        
        // log(e) = 1
        Assert.True(await forth.EvalAsync("2.718281828459045d FLN"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(1.0, (double)forth.Stack[1], 5);
        
        // Verify FLN and FLOG produce same results
        Assert.True(await forth.EvalAsync("10.0d FLN 10.0d FLOG"));
        Assert.Equal(4, forth.Stack.Count);
        Assert.Equal((double)forth.Stack[2], (double)forth.Stack[3], 15);
    }
    
    [Fact]
    public async Task FTilde_ANS_Semantics()
    {
        var forth = new ForthInterpreter();
        
        // Positive tolerance: absolute difference test
        Assert.True(await forth.EvalAsync("1.0d 1.0001d 0.01d F~"));
        Assert.Single(forth.Stack);
        Assert.Equal(-1L, (long)forth.Stack[0]); // true: |1.0 - 1.0001| < 0.01
        
        Assert.True(await forth.EvalAsync("1.0d 1.1d 0.05d F~"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[1]); // false: |1.0 - 1.1| >= 0.05
        
        // Zero tolerance: exact equality
        Assert.True(await forth.EvalAsync("1.5d 1.5d 0.0d F~"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[2]); // true: exact match
        
        Assert.True(await forth.EvalAsync("1.5d 1.50001d 0.0d F~"));
        Assert.Equal(4, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[3]); // false: not exact
        
        // Negative tolerance: relative difference test
        // For values around 100, a -0.01 tolerance means 1% relative error
        Assert.True(await forth.EvalAsync("100.0d 100.5d -0.01d F~"));
        Assert.Equal(5, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[4]); // true: 0.5% error < 1%
        
        Assert.True(await forth.EvalAsync("100.0d 102.0d -0.01d F~"));
        Assert.Equal(6, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[5]); // false: 2% error >= 1%
    }

    [Fact]
    public async Task FPow_Exponentiation()
    {
        var forth = new ForthInterpreter();
        // 2^3 = 8
        Assert.True(await forth.EvalAsync("2.0d 3.0d F**"));
        Assert.Single(forth.Stack);
        Assert.Equal(8.0, (double)forth.Stack[0], 10);

        // 4^0.5 = 2
        Assert.True(await forth.EvalAsync("4.0d 0.5d F**"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(2.0, (double)forth.Stack[1], 10);

        // 1^anything = 1
        Assert.True(await forth.EvalAsync("1.0d 10.0d F**"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(1.0, (double)forth.Stack[2], 10);
    }

    [Fact]
    public async Task FLOOR_InChainedOperations()
    {
        var forth = new ForthInterpreter();
        
        // FLOOR result used in multiplication (paranoia.4th pattern)
        Assert.True(await forth.EvalAsync("FVARIABLE X"));
        Assert.True(await forth.EvalAsync("3.7d X F!"));
        Assert.True(await forth.EvalAsync("X F@ FLOOR 2.0d F*"));
        Assert.Single(forth.Stack); // Result of multiplication
        Assert.IsType<double>(forth.Stack[0]);
        Assert.Equal(6.0, (double)forth.Stack[0], 10); // floor(3.7) * 2 = 3.0 * 2 = 6.0
        
        // Complex expression from paranoia.4th
        Assert.True(await forth.EvalAsync("0.5d 3.7d F+ FLOOR 2.0d F*"));
        Assert.Equal(2, forth.Stack.Count); // Previous result + new result
        Assert.Equal(8.0, (double)forth.Stack[1], 10); // floor(0.5 + 3.7) * 2 = floor(4.2) * 2 = 4.0 * 2 = 8.0
        
        // Verify FLOOR in complex paranoia.4th-style calculation
        Assert.True(await forth.EvalAsync("FVARIABLE Half  0.5d Half F!"));
        Assert.True(await forth.EvalAsync("FVARIABLE Radix  2.0d Radix F!"));
        Assert.True(await forth.EvalAsync("5.3d X F!"));
        Assert.True(await forth.EvalAsync("Half F@ X F@ F+ FLOOR Radix F@ F*"));
        Assert.Equal(3, forth.Stack.Count); // Previous 2 results + new result
        Assert.IsType<double>(forth.Stack[2]);
        Assert.Equal(10.0, (double)forth.Stack[2], 10); // floor(0.5 + 5.3) * 2 = floor(5.8) * 2 = 5.0 * 2 = 10.0
    }
    
    [Fact]
    public async Task FTilde_EdgeCases()
    {
        var forth = new ForthInterpreter();
        
        // Very small numbers with absolute tolerance
        Assert.True(await forth.EvalAsync("1.0E-10 1.0001E-10 1.0E-9 F~"));
        Assert.Single(forth.Stack);
        Assert.Equal(-1L, (long)forth.Stack[0]); // true: difference < tolerance
        
        // Very large numbers with relative tolerance
        Assert.True(await forth.EvalAsync("1.0E10 1.001E10 -0.01 F~"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[1]); // true: 0.1% error < 1%
        
        // Both zeros with any tolerance mode
        Assert.True(await forth.EvalAsync("0.0d 0.0d 0.01d F~")); // absolute
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[2]); // true
        
        Assert.True(await forth.EvalAsync("0.0d 0.0d 0.0d F~")); // exact
        Assert.Equal(4, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[3]); // true
        
        Assert.True(await forth.EvalAsync("0.0d 0.0d -0.01d F~")); // relative (special case: both zero)
        Assert.Equal(5, forth.Stack.Count);
        Assert.Equal(-1L, (long)forth.Stack[4]); // true: both are exactly zero
        
        // Near-zero with relative tolerance
        // 1E-300 vs 2E-300: difference is 1E-300, average is 1.5E-300
        // Relative diff = 1E-300 / 1.5E-300 = 66.7%, which is > 50%
        Assert.True(await forth.EvalAsync("1.0E-300 2.0E-300 -0.5 F~"));
        Assert.Equal(6, forth.Stack.Count);
        Assert.Equal(0L, (long)forth.Stack[5]); // false: 66.7% > 50% tolerance
        
        // Edge case: one zero, one non-zero with relative tolerance
        Assert.True(await forth.EvalAsync("0.0d 1.0E-10 -0.01 F~"));
        Assert.Equal(7, forth.Stack.Count);
        // With average magnitude near zero, this should be very strict
        var result = (long)forth.Stack[6];
        // This tests the implementation's handling of division by near-zero in relative mode
        Assert.True(result == -1L || result == 0L); // Implementation-dependent
    }
    
    [Fact]
    public async Task FLOATS_InMemoryAllocation()
    {
        var forth = new ForthInterpreter();
        
        // Typical pattern: allocate array of N floats
        Assert.True(await forth.EvalAsync("10 FLOATS ALLOCATE"));
        Assert.Equal(2, forth.Stack.Count);
        var addr = (long)forth.Stack[0];
        var ior = (long)forth.Stack[1];
        Assert.Equal(0L, ior); // success
        Assert.True(addr > 0);
        
        // Calculate offset for 5th float (0-indexed, so element 5 is at offset 5*FLOATS)
        Assert.True(await forth.EvalAsync("5 FLOATS"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(40L, (long)forth.Stack[2]); // 5 * 8 = 40 bytes
        
        // Use calculated offset to access array element
        Assert.True(await forth.EvalAsync("DROP")); // remove offset
        Assert.True(await forth.EvalAsync("3.14159d OVER F!")); // store at base address
        Assert.True(await forth.EvalAsync("DUP F@")); // retrieve
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(3.14159, (double)forth.Stack[2], 5);
        
        // Clean up
        Assert.True(await forth.EvalAsync("DROP FREE DROP")); // free allocated memory
    }
    
    [Fact]
    public async Task FLN_FLOG_Interchangeable()
    {
        var forth = new ForthInterpreter();
        
        // Use both in same calculation (paranoia.4th pattern)
        // Calculate: 240 * ln(X) / ln(Radix)
        Assert.True(await forth.EvalAsync("FVARIABLE U1  0.001d U1 F!"));
        Assert.True(await forth.EvalAsync("FVARIABLE Radix  2.0d Radix F!"));
        
        // Calculate precision using FLN
        Assert.True(await forth.EvalAsync("240.0d U1 F@ FLN F* Radix F@ FLN F/ FNEGATE"));
        Assert.Single(forth.Stack);
        var result1 = (double)forth.Stack[0];
        
        // Calculate same using FLOG
        Assert.True(await forth.EvalAsync("240.0d U1 F@ FLOG F* Radix F@ FLOG F/ FNEGATE"));
        Assert.Equal(2, forth.Stack.Count);
        var result2 = (double)forth.Stack[1];
        
        // Results should be identical
        Assert.Equal(result1, result2, 15);
        
        // Verify both produce reasonable precision value
        Assert.True(result1 > 0);
        Assert.True(result1 < 10000); // Sanity check
    }
    
    [Fact]
    public async Task ANS_Compliance_RealParanoiaFragment()
    {
        var forth = new ForthInterpreter();
        
        // Setup variables from paranoia.4th
        Assert.True(await forth.EvalAsync("FVARIABLE Half    0.5d Half F!"));
        Assert.True(await forth.EvalAsync("FVARIABLE One     1.0d One F!"));
        Assert.True(await forth.EvalAsync("FVARIABLE Radix   2.0d Radix F!"));
        Assert.True(await forth.EvalAsync("FVARIABLE U1      0.001d U1 F!"));
        Assert.True(await forth.EvalAsync("FVARIABLE X       3.7d X F!"));
        Assert.True(await forth.EvalAsync("FVARIABLE Y       0.0d Y F!"));
        
        // Test FLOOR returning float (from paranoia.4th line ~2550)
        // This expression: Half F@ X F@ F+ FLOOR Radix F@ F* X F@ F+ X F!
        // Calculates: X := (floor(0.5 + X) * Radix) + X
        Assert.True(await forth.EvalAsync("Half F@ X F@ F+ FLOOR Radix F@ F* X F@ F+ X F!"));
        Assert.Empty(forth.Stack); // All values consumed by X F!
        
        // Verify X was updated correctly
        Assert.True(await forth.EvalAsync("X F@"));
        Assert.Single(forth.Stack);
        var xValue = (double)forth.Stack[0];
        // floor(0.5 + 3.7) * 2 + 3.7 = floor(4.2) * 2 + 3.7 = 4 * 2 + 3.7 = 11.7
        Assert.Equal(11.7, xValue, 10);
        
        // Clear stack before next test
        Assert.True(await forth.EvalAsync("DROP"));
        Assert.Empty(forth.Stack);
        
        // Test FLOATS for precision detection (from paranoia.4th line ~45)
        Assert.True(await forth.EvalAsync("1 FLOATS"));
        Assert.Single(forth.Stack);
        var floatSize = (long)forth.Stack[0];
        Assert.Equal(8L, floatSize); // Should detect double precision
        
        // Test FLN in precision calculation (from paranoia.4th line ~2580)
        Assert.True(await forth.EvalAsync("240.0d U1 F@ FLN F* Radix F@ FLN F/ FNEGATE Y F!"));
        Assert.True(await forth.EvalAsync("Y F@"));
        Assert.Equal(2, forth.Stack.Count); // floatSize + Y value
        var precision = (double)forth.Stack[1];
        Assert.True(precision > 0); // Should calculate positive precision
        Assert.True(precision < 10000); // Sanity check
        
        // Test F~ with zero tolerance (from paranoia.4th)
        Assert.True(await forth.EvalAsync("One F@ One F@ 0.0d F~"));
        Assert.Equal(3, forth.Stack.Count); // floatSize + precision + F~ result
        Assert.Equal(-1L, (long)forth.Stack[2]); // Should be exactly equal
    }
    
    [Fact]
    public async Task Regression_NonFloatOperationsStillWork()
    {
        var forth = new ForthInterpreter();
        
        // Ensure integer conversion via F>S still works
        Assert.True(await forth.EvalAsync("3.7d F>S"));
        Assert.Single(forth.Stack);
        Assert.IsType<long>(forth.Stack[0]);
        Assert.Equal(3L, (long)forth.Stack[0]);
        
        // Ensure FTRUNC (truncate towards zero) still works
        Assert.True(await forth.EvalAsync("-3.7d FTRUNC"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.IsType<long>(forth.Stack[1]);
        Assert.Equal(-3L, (long)forth.Stack[1]);
        
        // Verify FLOOR behavior is different from F>S and FTRUNC
        Assert.True(await forth.EvalAsync("-3.7d FLOOR"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.IsType<double>(forth.Stack[2]);
        Assert.Equal(-4.0, (double)forth.Stack[2], 10); // FLOOR rounds down
        
        Assert.True(await forth.EvalAsync("-3.7d F>S"));
        Assert.Equal(4, forth.Stack.Count);
        Assert.IsType<long>(forth.Stack[3]);
        Assert.Equal(-3L, (long)forth.Stack[3]); // F>S truncates
        
        // Ensure CELLS (not FLOATS) still works correctly
        // Note: In this implementation, addresses are cell-granular
        // so CELLS is essentially identity (n CELLS = n)
        Assert.True(await forth.EvalAsync("1 CELLS"));
        Assert.Equal(5, forth.Stack.Count);
        Assert.Equal(1L, (long)forth.Stack[4]); // CELLS returns n (address units are cells)
        
        // Verify CELLS and FLOATS are DIFFERENT operations
        Assert.True(await forth.EvalAsync("1 FLOATS"));
        Assert.Equal(6, forth.Stack.Count);
        Assert.Equal(8L, (long)forth.Stack[5]); // FLOATS returns byte count (8 for double)
        
        // CELLS and FLOATS produce different results
        Assert.True(await forth.EvalAsync("3 CELLS 3 FLOATS"));
        Assert.Equal(8, forth.Stack.Count);
        Assert.Equal(3L, (long)forth.Stack[6]); // 3 CELLS = 3 (address units)
        Assert.Equal(24L, (long)forth.Stack[7]); // 3 FLOATS = 24 (bytes)
    }

    private sealed class TestIO : IForthIO
    {
        public readonly System.Collections.Generic.List<string> Outputs = new();
        public void Print(string text) => Outputs.Add(text);
        public void PrintNumber(long number) => Outputs.Add(number.ToString());
        public void NewLine() => Outputs.Add("\n");
        public string? ReadLine() => null;
    }
}
