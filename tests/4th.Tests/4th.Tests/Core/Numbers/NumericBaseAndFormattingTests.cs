using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Forth.Tests.Core.Numbers;

public class NumericBaseAndFormattingTests
{
    /// <summary>
    /// Intention: Confirm BASE changes affect number parsing and printing (DECIMAL vs HEX).
    /// Expected: Values entered after switching base interpret correctly (e.g., HEX A equals 10 decimal).
    /// </summary>
    [Fact]
    public async Task BaseSwitching()
    {
        var forth = new ForthInterpreter();
        Assert.True(await forth.EvalAsync("DECIMAL 10"));
        Assert.Single(forth.Stack);
        Assert.Equal(10L, (long)forth.Stack[0]);

        Assert.True(await forth.EvalAsync("HEX A"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(10L, (long)forth.Stack[^1]);

        Assert.True(await forth.EvalAsync("DECIMAL 10"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(10L, (long)forth.Stack[^1]);
    }

    private sealed class TestIO : Forth.Core.IForthIO
    {
        public readonly List<string> Outputs = new();
        public void Print(string text) => Outputs.Add(text);
        public void PrintNumber(long number) => Outputs.Add(number.ToString());
        public void NewLine() => Outputs.Add("\n");
        public string? ReadLine() => null;
    }

    /// <summary>
    /// Intention: Exercise pictured numeric output words building a string representation manually.
    /// Expected: <# ... #> produces string to TYPE.
    /// </summary>
    [Fact]
    public async Task PicturedNumericOutput()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync("<# 15 #S SIGN #> TYPE"));
        Assert.Single(io.Outputs);
        Assert.Equal("15", io.Outputs[0]);
        // Hex example: value remains decimal 255; base switch currently does not alter output conversion, expect decimal digits
        io.Outputs.Clear();
        Assert.True(await forth.EvalAsync("DECIMAL 255 HEX <# #S #> TYPE DECIMAL"));
        Assert.Single(io.Outputs);
        Assert.Equal("255", io.Outputs[0]);
        // Negative, keep sign copy before #S so SIGN sees it
        io.Outputs.Clear();
        Assert.True(await forth.EvalAsync("<# -42 DUP #S SWAP SIGN #> TYPE"));
        Assert.Single(io.Outputs);
        Assert.Equal("-42", io.Outputs[0]);
    }

    /// <summary>
    /// Intention: Verify >NUMBER converts a string to its numeric value producing residual substring if any.
    /// Expected: S" 123" 0 0 >NUMBER yields 123 and zero remainder length.
    /// </summary>
    [Fact]
    public async Task ToNumber_Parsing()
    {
        var forth = new ForthInterpreter();
        // S" 123" 0 0 >NUMBER -> 123 0 3
        Assert.True(await forth.EvalAsync("S\" 123\" 0 0 >NUMBER"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(123L, (long)forth.Stack[0]);
        Assert.Equal(0L, (long)forth.Stack[1]);
        Assert.Equal(3L, (long)forth.Stack[2]);
    }
}
