using Forth.Core.Interpreter;
using Xunit;
using Xunit.Abstractions;
using System.Threading.Tasks;

namespace Forth.Tests.Core.Numbers;

/// <summary>
/// Isolation tests to debug the >NUMBER first-digit loss issue
/// </summary>
public class ToNumberIsolationTests
{
    private readonly ITestOutputHelper _output;

    public ToNumberIsolationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static ForthInterpreter New() => new();

    [Fact]
    public async Task SQuote_MemoryLayout_Verification()
    {
        var f = New();
        await f.EvalAsync("S\" 123\"");
        
        Assert.Equal(2, f.Stack.Count);
        var caddr = (long)f.Stack[0];
        var u = (long)f.Stack[1];
        
        _output.WriteLine($"c-addr: {caddr}, u: {u}");
        
        // Verify length
        Assert.Equal(3L, u);
        
        // Read the actual characters from memory
        f.MemTryGet(caddr, out var ch1);
        f.MemTryGet(caddr + 1, out var ch2);
        f.MemTryGet(caddr + 2, out var ch3);
        
        _output.WriteLine($"Memory at c-addr: '{(char)ch1}' (code {ch1})");
        _output.WriteLine($"Memory at c-addr+1: '{(char)ch2}' (code {ch2})");
        _output.WriteLine($"Memory at c-addr+2: '{(char)ch3}' (code {ch3})");
        
        // Verify characters are correct
        Assert.Equal((long)'1', ch1);
        Assert.Equal((long)'2', ch2);
        Assert.Equal((long)'3', ch3);
        
        // Also check what's at caddr-1 (should be the length byte)
        f.MemTryGet(caddr - 1, out var lenByte);
        _output.WriteLine($"Memory at c-addr-1 (length byte): {lenByte}");
        Assert.Equal(3L, lenByte);
    }

    [Fact]
    public async Task ReadMemoryString_DirectTest()
    {
        var f = New();
        await f.EvalAsync("S\" ABC\"");
        
        var caddr = (long)f.Stack[0];
        var u = (long)f.Stack[1];
        
        _output.WriteLine($"c-addr: {caddr}, u: {u}");
        
        // Use ReadMemoryString directly
        var str = f.ReadMemoryString(caddr, u);
        _output.WriteLine($"ReadMemoryString result: '{str}' (length {str.Length})");
        
        Assert.Equal("ABC", str);
        Assert.Equal(3, str.Length);
    }

    [Fact]
    public async Task ToNumber_StepByStep_SingleDigit()
    {
        var f = New();
        // Test with single digit
        await f.EvalAsync("S\" 5\" 0 0 >NUMBER");
        
        Assert.Equal(3, f.Stack.Count);
        var acc = (long)f.Stack[0];
        var remFlag = (long)f.Stack[1];
        var consumed = (long)f.Stack[2];
        
        _output.WriteLine($"Single digit '5': acc={acc}, remFlag={remFlag}, consumed={consumed}");
        
        Assert.Equal(5L, acc);
        Assert.Equal(0L, remFlag);
        Assert.Equal(1L, consumed);
    }

    [Fact]
    public async Task ToNumber_StepByStep_TwoDigits()
    {
        var f = New();
        await f.EvalAsync("S\" 12\" 0 0 >NUMBER");
        
        Assert.Equal(3, f.Stack.Count);
        var acc = (long)f.Stack[0];
        var remFlag = (long)f.Stack[1];
        var consumed = (long)f.Stack[2];
        
        _output.WriteLine($"Two digits '12': acc={acc}, remFlag={remFlag}, consumed={consumed}");
        
        Assert.Equal(12L, acc);
        Assert.Equal(0L, remFlag);
        Assert.Equal(2L, consumed);
    }

    [Fact]
    public async Task ToNumber_StepByStep_ThreeDigits()
    {
        var f = New();
        await f.EvalAsync("S\" 123\" 0 0 >NUMBER");
        
        Assert.Equal(3, f.Stack.Count);
        var acc = (long)f.Stack[0];
        var remFlag = (long)f.Stack[1];
        var consumed = (long)f.Stack[2];
        
        _output.WriteLine($"Three digits '123': acc={acc}, remFlag={remFlag}, consumed={consumed}");
        
        Assert.Equal(123L, acc);
        Assert.Equal(0L, remFlag);
        Assert.Equal(3L, consumed);
    }

    [Fact]
    public async Task ToNumber_WithLeadingSpace()
    {
        var f = New();
        // Note: Tokenizer should skip the space after S"
        await f.EvalAsync("S\" 456\" 0 0 >NUMBER");
        
        Assert.Equal(3, f.Stack.Count);
        var acc = (long)f.Stack[0];
        var remFlag = (long)f.Stack[1];
        var consumed = (long)f.Stack[2];
        
        _output.WriteLine($"Digits '456' (space skipped by tokenizer): acc={acc}, remFlag={remFlag}, consumed={consumed}");
        
        Assert.Equal(456L, acc);
        Assert.Equal(0L, remFlag);
        Assert.Equal(3L, consumed);
    }

    [Fact]
    public async Task ToNumber_HexSingleDigit()
    {
        var f = New();
        await f.EvalAsync("HEX S\" F\" 0 0 >NUMBER DECIMAL");
        
        Assert.Equal(3, f.Stack.Count);
        var acc = (long)f.Stack[0];
        var remFlag = (long)f.Stack[1];
        var consumed = (long)f.Stack[2];
        
        _output.WriteLine($"Hex 'F': acc={acc}, remFlag={remFlag}, consumed={consumed}");
        
        Assert.Equal(15L, acc);
        Assert.Equal(0L, remFlag);
        Assert.Equal(1L, consumed);
    }

    [Fact]
    public async Task ToNumber_HexTwoDigits()
    {
        var f = New();
        await f.EvalAsync("HEX S\" FF\" 0 0 >NUMBER DECIMAL");
        
        Assert.Equal(3, f.Stack.Count);
        var acc = (long)f.Stack[0];
        var remFlag = (long)f.Stack[1];
        var consumed = (long)f.Stack[2];
        
        _output.WriteLine($"Hex 'FF': acc={acc}, remFlag={remFlag}, consumed={consumed}");
        
        Assert.Equal(255L, acc);
        Assert.Equal(0L, remFlag);
        Assert.Equal(2L, consumed);
    }

    [Fact]
    public async Task ToNumber_Accumulator_NonZero()
    {
        var f = New();
        // Start with acc=10, should become 10*10 + 5 = 105
        await f.EvalAsync("S\" 5\" 10 0 >NUMBER");
        
        Assert.Equal(3, f.Stack.Count);
        var acc = (long)f.Stack[0];
        var remFlag = (long)f.Stack[1];
        var consumed = (long)f.Stack[2];
        
        _output.WriteLine($"Accumulator test (start=10, '5'): acc={acc}, remFlag={remFlag}, consumed={consumed}");
        
        Assert.Equal(105L, acc);
        Assert.Equal(0L, remFlag);
        Assert.Equal(1L, consumed);
    }

    [Fact]
    public async Task ToNumber_StartCount_NonZero()
    {
        var f = New();
        // Start with consumed=2, should return consumed=5 (2+3)
        await f.EvalAsync("S\" 789\" 0 2 >NUMBER");
        
        Assert.Equal(3, f.Stack.Count);
        var acc = (long)f.Stack[0];
        var remFlag = (long)f.Stack[1];
        var consumed = (long)f.Stack[2];
        
        _output.WriteLine($"Start count test (start=2, '789'): acc={acc}, remFlag={remFlag}, consumed={consumed}");
        
        Assert.Equal(789L, acc);
        Assert.Equal(0L, remFlag);
        Assert.Equal(5L, consumed); // 2 + 3 = 5
    }

    [Fact]
    public async Task SQuote_NoSpace_MemoryLayout()
    {
        var f = New();
        // Test without space after S"
        await f.EvalAsync("S\"XYZ\"");
        
        Assert.Equal(2, f.Stack.Count);
        var caddr = (long)f.Stack[0];
        var u = (long)f.Stack[1];
        
        _output.WriteLine($"No space case - c-addr: {caddr}, u: {u}");
        
        // Read the actual characters
        f.MemTryGet(caddr, out var ch1);
        f.MemTryGet(caddr + 1, out var ch2);
        f.MemTryGet(caddr + 2, out var ch3);
        
        _output.WriteLine($"Memory: '{(char)ch1}', '{(char)ch2}', '{(char)ch3}'");
        
        Assert.Equal((long)'X', ch1);
        Assert.Equal((long)'Y', ch2);
        Assert.Equal((long)'Z', ch3);
    }

    [Fact]
    public Task Tokenizer_Output_Verification()
    {
        // Test what the tokenizer produces
        var tokens = Tokenizer.Tokenize("S\" 123\" 0 0 >NUMBER");
        
        _output.WriteLine($"Token count: {tokens.Count}");
        for (int i = 0; i < tokens.Count; i++)
        {
            _output.WriteLine($"Token[{i}]: '{tokens[i]}' (length {tokens[i].Length})");
        }
        
        Assert.Equal(5, tokens.Count);
        Assert.Equal("S\"", tokens[0]);
        Assert.Equal("\"123\"", tokens[1]);
        Assert.Equal("0", tokens[2]);
        Assert.Equal("0", tokens[3]);
        Assert.Equal(">NUMBER", tokens[4]);
        
        // Verify the string token content
        var strToken = tokens[1];
        Assert.Equal('"', strToken[0]);
        Assert.Equal('"', strToken[^1]);
        var content = strToken[1..^1];
        _output.WriteLine($"String content after removing quotes: '{content}'");
        Assert.Equal("123", content);
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ToUNumber_ParseUnsignedNumber()
    {
        var f = New();
        await f.EvalAsync("S\" 123\" >UNUMBER");
        
        Assert.Single(f.Stack);
        var num = (long)f.Stack[0];
        
        Assert.Equal(123L, num);
    }

    [Fact]
    public async Task ToUNumber_ParseHexUnsignedNumber()
    {
        var f = New();
        await f.EvalAsync("HEX S\" FF\" >UNUMBER DECIMAL");
        
        Assert.Single(f.Stack);
        var num = (long)f.Stack[0];
        
        Assert.Equal(255L, num);
    }

    [Fact]
    public async Task ToUNumber_InvalidNumber()
    {
        var f = New();
        await f.EvalAsync("S\" ABC\" >UNUMBER");
        
        Assert.Equal(3, f.Stack.Count);
        var flag = (long)f.Stack[2];
        
        Assert.Equal(0L, flag); // failure flag
    }
}
