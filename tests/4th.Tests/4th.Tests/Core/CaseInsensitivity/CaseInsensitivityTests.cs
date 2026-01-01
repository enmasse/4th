using Xunit;
using Forth.Core.Interpreter;

namespace Forth.Tests.Core.CaseInsensitivity;

public class CaseInsensitivityTests
{
    [Fact]
    public async Task SQuote_LowercaseWorks()
    {
        var interp = new ForthInterpreter();
        
        // Lowercase s" should work the same as uppercase S"
        await interp.EvalAsync(@"s"" test"" ");
        
        Assert.Equal(2, interp.Stack.Count);
        Assert.IsType<long>(interp.Stack[0]); // c-addr
        Assert.IsType<long>(interp.Stack[1]); // u
        
        var len = (long)interp.Stack[1];
        Assert.Equal(4, len);
    }
    
    [Fact]
    public async Task PrimitivesAreCaseInsensitive()
    {
        var interp = new ForthInterpreter();
        
        // Test that basic primitives work in any case
        await interp.EvalAsync("5 DUP");
        Assert.Equal(2, interp.Stack.Count);
        
        interp = new ForthInterpreter();
        await interp.EvalAsync("5 dup");
        Assert.Equal(2, interp.Stack.Count);
        
        interp = new ForthInterpreter();
        await interp.EvalAsync("5 Dup");
        Assert.Equal(2, interp.Stack.Count);
    }
    
    [Fact]
    public async Task DefinedWordsAreCaseInsensitive()
    {
        var interp = new ForthInterpreter();
        
        // Define with uppercase
        await interp.EvalAsync(": TESTWORD 42 ;");
        
        // Call with lowercase
        await interp.EvalAsync("testword");
        Assert.Single(interp.Stack);
        Assert.Equal(42L, (long)interp.Stack[0]);
        
        var top = interp.Pop();
        
        // Call with mixed case
        await interp.EvalAsync("TestWord");
        Assert.Single(interp.Stack);
        Assert.Equal(42L, (long)interp.Stack[0]);
    }
}
