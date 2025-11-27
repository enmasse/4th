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
    public async Task Tokenizer_Output_Verification()
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
    }

    [Fact]
    public async Task Manual_Memory_Allocation_Test()
    {
        var f = New();
        
        // Manually allocate a counted string
        var addr = f.AllocateCountedString("999");
        _output.WriteLine($"AllocateCountedString('999') returned addr: {addr}");
        
        // Check the length byte
        f.MemTryGet(addr, out var len);
        _output.WriteLine($"Length byte at addr {addr}: {len}");
        Assert.Equal(3L, len);
        
        // Check the characters
        f.MemTryGet(addr + 1, out var ch1);
        f.MemTryGet(addr + 2, out var ch2);
        f.MemTryGet(addr + 3, out var ch3);
        
        _output.WriteLine($"Chars: '{(char)ch1}' '{(char)ch2}' '{(char)ch3}'");
        Assert.Equal((long)'9', ch1);
        Assert.Equal((long)'9', ch2);
        Assert.Equal((long)'9', ch3);
        
        // Now manually push c-addr and u, then call >NUMBER
        f.Push(addr + 1); // c-addr
        f.Push(3L);       // u
        f.Push(0L);       // acc
        f.Push(0L);       // start
        
        await f.EvalAsync(">NUMBER");
        
        Assert.Equal(3, f.Stack.Count);
        var acc = (long)f.Stack[0];
        var remFlag = (long)f.Stack[1];
        var consumed = (long)f.Stack[2];
        
        _output.WriteLine($"Manual test '999': acc={acc}, remFlag={remFlag}, consumed={consumed}");
        
        Assert.Equal(999L, acc);
        Assert.Equal(0L, remFlag);
        Assert.Equal(3L, consumed);
    }

    [Fact]
    public async Task SQuote_DirectStackInspection()
    {
        var f = New();
        await f.EvalAsync("S\" ABC\"");
        
        Assert.Equal(2, f.Stack.Count);
        var caddr = f.Stack[0];
        var u = f.Stack[1];
        
        _output.WriteLine($"S\" pushes: caddr={caddr} (type={caddr.GetType().Name}), u={u} (type={u.GetType().Name})");
        
        Assert.IsType<long>(caddr);
        Assert.IsType<long>(u);
        
        var caddrLong = (long)caddr;
        var uLong = (long)u;
        
        _output.WriteLine($"Values: caddr={caddrLong}, u={uLong}");
        
        // Read what's at caddr-1 (should be length byte)
        f.MemTryGet(caddrLong - 1, out var lenByte);
        _output.WriteLine($"Memory at caddr-1: {lenByte}");
        
        // Read what's at caddr, caddr+1, caddr+2
        f.MemTryGet(caddrLong, out var ch0);
        f.MemTryGet(caddrLong + 1, out var ch1);
        f.MemTryGet(caddrLong + 2, out var ch2);
        
        _output.WriteLine($"Memory at caddr+0: '{(char)ch0}' (code {ch0})");
        _output.WriteLine($"Memory at caddr+1: '{(char)ch1}' (code {ch1})");
        _output.WriteLine($"Memory at caddr+2: '{(char)ch2}' (code {ch2})");
        
        Assert.Equal(3L, uLong);
        Assert.Equal((long)'A', ch0);
        Assert.Equal((long)'B', ch1);
        Assert.Equal((long)'C', ch2);
    }

    [Fact]
    public async Task ToNumber_Stack_Inspection_Before_Call()
    {
        var f = New();
        // Execute S" but not >NUMBER yet
        await f.EvalAsync("S\" 12\"");
        
        Assert.Equal(2, f.Stack.Count);
        var val0 = f.Stack[0];
        var val1 = f.Stack[1];
        
        _output.WriteLine($"After S\" 12\": Stack[0]={val0} (type={val0.GetType().Name}), Stack[1]={val1} (type={val1.GetType().Name})");
        
        // Now add the remaining arguments and call >NUMBER
        await f.EvalAsync("0 0 >NUMBER");
        
        Assert.Equal(3, f.Stack.Count);
        var acc = (long)f.Stack[0];
        var remFlag = (long)f.Stack[1];
        var consumed = (long)f.Stack[2];
        
        _output.WriteLine($"After >NUMBER: acc={acc}, remFlag={remFlag}, consumed={consumed}");
        
        Assert.Equal(12L, acc);
        Assert.Equal(0L, remFlag);
        Assert.Equal(2L, consumed);
    }

    [Fact]
    public async Task SQuote_Twelve_Memory_Inspection()
    {
        var f = New();
        await f.EvalAsync("S\" 12\"");
        
        Assert.Equal(2, f.Stack.Count);
        var caddr = (long)f.Stack[0];
        var u = (long)f.Stack[1];
        
        _output.WriteLine($"S\" 12\" pushes: caddr={caddr}, u={u}");
        
        // Check length byte at caddr-1
        f.MemTryGet(caddr - 1, out var lenByte);
        _output.WriteLine($"Memory at caddr-1 (length byte): {lenByte}");
        
        // Check characters at caddr and caddr+1
        f.MemTryGet(caddr, out var ch0);
        f.MemTryGet(caddr + 1, out var ch1);
        
        _output.WriteLine($"Memory at caddr+0: '{(char)ch0}' (code {ch0})");
        _output.WriteLine($"Memory at caddr+1: '{(char)ch1}' (code {ch1})");
        
        // Use ReadMemoryString to get the full string
        var str = f.ReadMemoryString(caddr, u);
        _output.WriteLine($"ReadMemoryString({caddr}, {u}) returns: '{str}'");
        
        Assert.Equal(2L, u);
        Assert.Equal(2L, lenByte);
        Assert.Equal((long)'1', ch0);
        Assert.Equal((long)'2', ch1);
        Assert.Equal("12", str);
    }

    [Fact]
    public async Task ToNumber_Single_vs_Multiple_Eval_Calls()
    {
        var f1 = New();
        // Single EvalAsync call (like the failing tests)
        await f1.EvalAsync("S\" 99\" 0 0 >NUMBER");
        
        Assert.Equal(3, f1.Stack.Count);
        var acc1 = (long)f1.Stack[0];
        var rem1 = (long)f1.Stack[1];
        var con1 = (long)f1.Stack[2];
        
        _output.WriteLine($"Single call: acc={acc1}, rem={rem1}, con={con1}");
        
        var f2 = New();
        // Multiple EvalAsync calls (like ToNumber_Stack_Inspection_Before_Call)
        await f2.EvalAsync("S\" 99\"");
        await f2.EvalAsync("0 0 >NUMBER");
        
        Assert.Equal(3, f2.Stack.Count);
        var acc2 = (long)f2.Stack[0];
        var rem2 = (long)f2.Stack[1];
        var con2 = (long)f2.Stack[2];
        
        _output.WriteLine($"Multiple calls: acc={acc2}, rem={rem2}, con={con2}");
        
        // Both should produce the same result
        Assert.Equal(99L, acc1);
        Assert.Equal(0L, rem1);
        Assert.Equal(2L, con1);
        
        Assert.Equal(99L, acc2);
        Assert.Equal(0L, rem2);
        Assert.Equal(2L, con2);
    }

    [Fact]
    public async Task ToNumber_Diagnostic_Simple()
    {
        var f = New();
        Console.WriteLine("TEST: About to call EvalAsync");
        await f.EvalAsync("S\" 99\" 0 0 >NUMBER");
        Console.WriteLine("TEST: EvalAsync returned");
        
        Assert.Equal(3, f.Stack.Count);
        var acc = (long)f.Stack[0];
        _output.WriteLine($"Result: acc={acc}");
        Assert.Equal(99L, acc);
    }

    [Fact]
    public void Tokenizer_Diagnostic()
    {
        var tokens = Tokenizer.Tokenize("S\" 99\" 0 0 >NUMBER");
        
        _output.WriteLine($"Token count: {tokens.Count}");
        for (int i = 0; i < tokens.Count; i++)
        {
            _output.WriteLine($"Token[{i}]: '{tokens[i]}'");
        }
        
        Assert.Equal(5, tokens.Count);
        Assert.Equal("S\"", tokens[0]);
        Assert.Equal("\"99\"", tokens[1]);
        Assert.Equal("0", tokens[2]);
        Assert.Equal("0", tokens[3]);
        Assert.Equal(">NUMBER", tokens[4]);
    }

    [Fact]
    public async Task Stack_After_Each_Token()
    {
        var f = New();
        
        // After S" 99"
        await f.EvalAsync("S\" 99\"");
        _output.WriteLine($"After S\" 99\": Stack.Count={f.Stack.Count}");
        for (int i = 0; i < f.Stack.Count; i++)
        {
            _output.WriteLine($"  Stack[{i}] = {f.Stack[i]} (type={f.Stack[i].GetType().Name})");
        }
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(6L, (long)f.Stack[0]);
        Assert.Equal(2L, (long)f.Stack[1]);
        
        // After first 0
        await f.EvalAsync("0");
        _output.WriteLine($"After 0: Stack.Count={f.Stack.Count}");
        for (int i = 0; i < f.Stack.Count; i++)
        {
            _output.WriteLine($"  Stack[{i}] = {f.Stack[i]} (type={f.Stack[i].GetType().Name})");
        }
        Assert.Equal(3, f.Stack.Count);
        Assert.Equal(6L, (long)f.Stack[0]);
        Assert.Equal(2L, (long)f.Stack[1]);
        Assert.Equal(0L, (long)f.Stack[2]);
        
        // After second 0
        await f.EvalAsync("0");
        _output.WriteLine($"After second 0: Stack.Count={f.Stack.Count}");
        for (int i = 0; i < f.Stack.Count; i++)
        {
            _output.WriteLine($"  Stack[{i}] = {f.Stack[i]} (type={f.Stack[i].GetType().Name})");
        }
        Assert.Equal(4, f.Stack.Count);
        Assert.Equal(6L, (long)f.Stack[0]);
        Assert.Equal(2L, (long)f.Stack[1]);
        Assert.Equal(0L, (long)f.Stack[2]);
        Assert.Equal(0L, (long)f.Stack[3]);
    }
}
