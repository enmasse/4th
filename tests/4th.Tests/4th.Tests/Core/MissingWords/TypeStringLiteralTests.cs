using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class TypeStringLiteralTests
{
    private sealed class TestIO : IForthIO
    {
        public readonly System.Collections.Generic.List<string> Outputs = new();
        public void Print(string text) => Outputs.Add(text);
        public void PrintNumber(long number) => Outputs.Add(number.ToString());
        public void NewLine() => Outputs.Add("\n");
        public string? ReadLine() => null;
    }

    [Fact]
    public async Task SQuote_InWordDefinition_PushesAddrAndLength()
    {
        var forth = new ForthInterpreter();
        // Define a word that uses S" and leaves the results on stack
        Assert.True(await forth.EvalAsync(": TEST-WORD S\" hello\" ;"));
        Assert.True(await forth.EvalAsync("TEST-WORD"));
        
        // Should have addr and length on stack
        Assert.Equal(2, forth.Stack.Count);
        var addr = (long)forth.Stack[0];
        var len = (long)forth.Stack[1];
        Assert.Equal(5L, len);
        var str = forth.ReadMemoryString(addr, len);
        Assert.Equal("hello", str);
    }

    [Fact]
    public async Task SQuote_InWordDefinition_TypeWorks()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        // Define a word that uses S" followed by TYPE
        Assert.True(await forth.EvalAsync(": PRINT-MSG S\" Test Message\" TYPE ;"));
        Assert.True(await forth.EvalAsync("PRINT-MSG"));
        
        // Check output
        Assert.Single(io.Outputs);
        Assert.Equal("Test Message", io.Outputs[0]);
    }

    [Fact]
    public async Task SQuote_MultipleInSameWord_Works()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        // Define a word with multiple S" calls
        Assert.True(await forth.EvalAsync(": MULTI S\" First\" TYPE S\" Second\" TYPE ;"));
        Assert.True(await forth.EvalAsync("MULTI"));
        
        Assert.Equal(2, io.Outputs.Count);
        Assert.Equal("First", io.Outputs[0]);
        Assert.Equal("Second", io.Outputs[1]);
    }

    [Fact]
    public async Task ErrorXT_Pattern_Works()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        // Simulate the ERROR-XT pattern from ttester.fs
        Assert.True(await forth.EvalAsync("VARIABLE ERROR-XT"));
        Assert.True(await forth.EvalAsync(": ERROR1 TYPE CR ;"));
        Assert.True(await forth.EvalAsync("' ERROR1 ERROR-XT !"));
        
        // Now test calling via ERROR
        Assert.True(await forth.EvalAsync(": ERROR ERROR-XT @ EXECUTE ;"));
        
        // Push a string and call ERROR
        Assert.True(await forth.EvalAsync("S\" Error message\" ERROR"));
        
        Assert.Equal(2, io.Outputs.Count);
        Assert.Equal("Error message", io.Outputs[0]);
        Assert.Equal("\n", io.Outputs[1]);
    }

    [Fact]
    public async Task SQuote_CalledViaVariable_Works()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        // This reproduces the exact pattern: S" followed by ERROR
        Assert.True(await forth.EvalAsync("VARIABLE ERROR-XT"));
        Assert.True(await forth.EvalAsync(": ERROR1 TYPE CR ;"));
        Assert.True(await forth.EvalAsync("' ERROR1 ERROR-XT !"));
        Assert.True(await forth.EvalAsync(": ERROR ERROR-XT @ EXECUTE ;"));
        
        // Define a word that calls S" then ERROR
        Assert.True(await forth.EvalAsync(": REPORT S\" FAIL: \" ERROR ;"));
        Assert.True(await forth.EvalAsync("REPORT"));
        
        Assert.Equal(2, io.Outputs.Count);
        Assert.Equal("FAIL: ", io.Outputs[0]);
        Assert.Equal("\n", io.Outputs[1]);
    }

    [Fact]
    public async Task SQuote_InCompoundWord_PreservesStack()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        // Verify S" inside a word definition preserves proper stack
        Assert.True(await forth.EvalAsync(": WRAPPER S\" text\" ;"));
        Assert.True(await forth.EvalAsync("WRAPPER .S"));
        
        Assert.Single(io.Outputs);
        // Should show 2 items on stack (addr and length)
        Assert.Contains("<2>", io.Outputs[0]);
    }

    [Fact]
    public async Task SQuote_AtTopLevel_PushesImmediately()
    {
        var forth = new ForthInterpreter();
        // At top level (interpret mode), S" should push immediately
        Assert.True(await forth.EvalAsync("S\" hello\""));
        Assert.Equal(2, forth.Stack.Count);
        var addr = (long)forth.Stack[0];
        var len = (long)forth.Stack[1];
        Assert.Equal(5L, len);
    }

    [Fact]
    public async Task SimpleWordWithType_Works()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        // Simple word that uses S" and TYPE directly
        Assert.True(await forth.EvalAsync(": TEST S\" hello\" TYPE ;"));
        Assert.True(await forth.EvalAsync("TEST"));
        
        Assert.Single(io.Outputs);
        Assert.Equal("hello", io.Outputs[0]);
    }
}
