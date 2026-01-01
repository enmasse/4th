using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core.Numbers;

/// <summary>
/// Tests for Forth scientific notation shorthand where 'E' or 'e' at the end
/// without an explicit exponent indicates a floating-point type.
/// Examples: 1.0E = 1.0, 2e = 2.0, 3.14E = 3.14
/// This notation is used in the paranoia.4th test suite.
/// </summary>
public class ScientificNotationShorthandTests
{
    [Fact]
    public async Task IntegerE_ParsesAsFloat()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1e"));
        Assert.Single(forth.Stack);
        Assert.IsType<double>(forth.Stack[0]);
        Assert.Equal(1.0, (double)forth.Stack[0]);
    }

    [Fact]
    public async Task IntegerUpperE_ParsesAsFloat()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("2E"));
        Assert.Single(forth.Stack);
        Assert.IsType<double>(forth.Stack[0]);
        Assert.Equal(2.0, (double)forth.Stack[0]);
    }

    [Fact]
    public async Task NegativeIntegerE_ParsesAsFloat()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("-3e"));
        Assert.Single(forth.Stack);
        Assert.IsType<double>(forth.Stack[0]);
        Assert.Equal(-3.0, (double)forth.Stack[0]);
    }

    [Fact]
    public async Task ZeroE_ParsesAsFloat()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("0e"));
        Assert.Single(forth.Stack);
        Assert.IsType<double>(forth.Stack[0]);
        Assert.Equal(0.0, (double)forth.Stack[0]);
    }

    [Fact]
    public async Task DecimalE_ParsesAsFloat()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1.0E"));
        Assert.Single(forth.Stack);
        Assert.IsType<double>(forth.Stack[0]);
        Assert.Equal(1.0, (double)forth.Stack[0]);
    }

    [Fact]
    public async Task DecimalLowercaseE_ParsesAsFloat()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("3.14e"));
        Assert.Single(forth.Stack);
        Assert.IsType<double>(forth.Stack[0]);
        Assert.Equal(3.14, (double)forth.Stack[0], 5);
    }

    [Fact]
    public async Task NegativeDecimalE_ParsesAsFloat()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("-2.5E"));
        Assert.Single(forth.Stack);
        Assert.IsType<double>(forth.Stack[0]);
        Assert.Equal(-2.5, (double)forth.Stack[0], 5);
    }

    [Fact]
    public async Task MultipleShorthandLiterals()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1e 2E 3.14e"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(1.0, (double)forth.Stack[0]);
        Assert.Equal(2.0, (double)forth.Stack[1]);
        Assert.Equal(3.14, (double)forth.Stack[2], 5);
    }

    [Fact]
    public async Task ShorthandE_WorksInArithmetic()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1.0E 2.0E F+"));
        Assert.Single(forth.Stack);
        Assert.Equal(3.0, (double)forth.Stack[0]);
    }

    [Fact]
    public async Task NormalScientificNotation_StillWorks()
    {
        var forth = new ForthInterpreter();
        // Verify that normal scientific notation (with explicit exponent) still works
        Assert.True(await forth.EvalAsync("1.5e2"));  // 1.5 * 10^2 = 150
        Assert.Single(forth.Stack);
        Assert.Equal(150.0, (double)forth.Stack[0]);
    }

    [Fact]
    public async Task NormalScientificNotation_NegativeExponent()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("2.5e-1"));  // 2.5 * 10^-1 = 0.25
        Assert.Single(forth.Stack);
        Assert.Equal(0.25, (double)forth.Stack[0]);
    }

    [Fact]
    public async Task DSuffix_ShorthandE_ParsesAsFloat()
    {
        var forth = new ForthInterpreter();
        // Test combination of D suffix with shorthand E
        Assert.True(await forth.EvalAsync("1.5Ed"));
        Assert.Single(forth.Stack);
        Assert.IsType<double>(forth.Stack[0]);
        Assert.Equal(1.5, (double)forth.Stack[0], 5);
    }

    [Fact]
    public async Task LargeIntegerE_ParsesAsFloat()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("1000E"));
        Assert.Single(forth.Stack);
        Assert.IsType<double>(forth.Stack[0]);
        Assert.Equal(1000.0, (double)forth.Stack[0]);
    }

    [Fact]
    public async Task FloatingArithmetic_WithShorthandE()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("3.14E 2.0E F* F."));
        // Stack should be empty after F.
        Assert.Empty(forth.Stack);
        // Output should be "6.28" (printed by F.)
    }

    [Fact]
    public async Task ShorthandE_InDefinition()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync(": TESTWORD 1.0E F. ;"));
        Assert.True(await forth.EvalAsync("TESTWORD"));
        // Should print "1" without error
        Assert.Empty(forth.Stack);
    }
}
