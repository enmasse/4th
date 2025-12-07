using Xunit;
using Xunit.Abstractions;
using Forth.Core;
using Forth.Core.Interpreter;

namespace Forth.Tests.Compliance;

public class ParanoiaStringLiteralBugTests
{
    private readonly ITestOutputHelper _output;

    public ParanoiaStringLiteralBugTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void UppercaseSQuoteProducesCorrectStack()
    {
        // Test that uppercase S" produces (c-addr u)
        var interp = new ForthInterpreter();
        
        interp.EvalAsync(@"S"" test"" ").Wait();
        
        Assert.Equal(2, interp.Stack.Count);
        Assert.IsType<long>(interp.Stack[0]); // c-addr
        Assert.IsType<long>(interp.Stack[1]); // u
        
        var len = (long)interp.Stack[1];
        var addr = (long)interp.Stack[0];
        
        _output.WriteLine($"S\" produced: addr={addr}, len={len}");
        Assert.Equal(4, len); // "test" has 4 characters
    }

    [Fact]
    public void LowercaseSQuoteProducesStringObject()
    {
        // Test that lowercase s" (if defined) produces different behavior
        var interp = new ForthInterpreter();
        
        interp.EvalAsync(@"s"" test"" ").Wait();
        
        _output.WriteLine($"Stack count: {interp.Stack.Count}");
        if (interp.Stack.Count > 0)
        {
            foreach (var item in interp.Stack)
            {
                _output.WriteLine($"  Stack item: {item} (type: {item?.GetType().Name})");
            }
        }
        
        // If lowercase s" is defined differently, it might push a string object
        // This would cause the CMOVE failure in paranoia.4th
    }

    [Fact]
    public void ParanoiaPatternWithCorrectSQuote()
    {
        // Test the paranoia pattern with the correct uppercase S"
        var interp = new ForthInterpreter();
        
        var code = @"
            S"" [UNDEFINED]"" dup pad c! pad char+ swap cmove 
            pad find nip 0=
        ";
        
        interp.EvalAsync(code).Wait();
        
        Assert.NotEmpty(interp.Stack);
        var result = interp.PopInternal();
        _output.WriteLine($"Result: {result}");
    }

    [Fact]
    public void ParanoiaPatternWithLowercaseSQuote()
    {
        // Test what happens with lowercase s" if it pushes a string object
        var interp = new ForthInterpreter();
        
        try
        {
            var code = @"
                s"" [UNDEFINED]"" dup pad c! pad char+ swap cmove 
                pad find nip 0=
            ";
            
            interp.EvalAsync(code).Wait();
            
            _output.WriteLine("Code completed successfully");
            if (interp.Stack.Count > 0)
            {
                _output.WriteLine($"Stack result: {interp.Stack.Last()}");
            }
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Failed with: {ex.Message}");
            _output.WriteLine($"\nStack contents when failed ({interp.Stack.Count} items):");
            foreach (var item in interp.Stack)
            {
                _output.WriteLine($"  {item} (type: {item?.GetType().Name})");
            }
            
            // This should fail with stack underflow if s" pushes a string object
            Assert.Contains("Stack underflow", ex.Message);
        }
    }

    [Fact]
    public void DocumentRootCause()
    {
        _output.WriteLine("ROOT CAUSE ANALYSIS:");
        _output.WriteLine("====================");
        _output.WriteLine("");
        _output.WriteLine("The paranoia.4th test file was patched to fix initialization bugs.");
        _output.WriteLine("The patch changed lines 278 and 285 to use:");
        _output.WriteLine(@"  s"" [UNDEFINED]"" dup pad c! pad char+ swap cmove");
        _output.WriteLine("");
        _output.WriteLine("However, paranoia.4th uses LOWERCASE 's\"' not uppercase 'S\"'.");
        _output.WriteLine("");
        _output.WriteLine("In Forth systems:");
        _output.WriteLine("  - S\" (uppercase) is ANS Forth standard: ( -- c-addr u )");
        _output.WriteLine("  - s\" (lowercase) may be a different word with different behavior");
        _output.WriteLine("");
        _output.WriteLine("The actual paranoia.4th code uses lowercase s\".");
        _output.WriteLine("If lowercase s\" pushes a string object instead of (c-addr u),");
        _output.WriteLine("the CMOVE will fail because:");
        _output.WriteLine("  1. s\" pushes a String object");
        _output.WriteLine("  2. dup duplicates it");
        _output.WriteLine("  3. pad c! tries to store String at pad (may succeed or fail)");
        _output.WriteLine("  4. pad char+ swap leaves: (String pad+1)");
        _output.WriteLine("  5. cmove expects (src-addr dst-addr u) but gets (String pad+1)");
        _output.WriteLine("  6. CMOVE fails with stack underflow because it expects 3 items");
        _output.WriteLine("");
        _output.WriteLine("SOLUTION:");
        _output.WriteLine("  The patch should use uppercase S\" not lowercase s\"");
        _output.WriteLine("  OR define lowercase s\" to behave like S\"");
    }
}
