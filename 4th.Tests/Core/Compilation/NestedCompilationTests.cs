using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using Xunit.Abstractions;

namespace Forth.Tests.Core.Compilation;

public class NestedCompilationTests
{
    private readonly ITestOutputHelper _output;

    public NestedCompilationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task CreateInsideColonDefinition_ShouldWork()
    {
        var forth = new ForthInterpreter();
        
        // Define a word that uses CREATE
        await forth.EvalAsync(": MAKER CREATE ;");
        
        // Use it to create a word
        await forth.EvalAsync("MAKER FOO");
        
        // Verify FOO was created
        var words = forth.GetAllWordNames();
        Assert.Contains("FOO", words, StringComparer.OrdinalIgnoreCase);
        
        // Execute FOO and verify it pushes an address
        await forth.EvalAsync("FOO");
        Assert.Single(forth.Stack);
        Assert.IsType<long>(forth.Stack[0]);
    }

    [Fact]
    public async Task CreateDupCommaInsideColonDefinition_ShouldWork()
    {
        var forth = new ForthInterpreter();
        
        // This pattern from ERROR-COUNT
        await forth.EvalAsync(": TEST-MAKER CREATE DUP , CELL+ ;");
        
        _output.WriteLine("TEST-MAKER defined");
        
        // Use it: start with value on stack
        await forth.EvalAsync("0 TEST-MAKER FIRST-WORD");
        
        _output.WriteLine($"Stack after TEST-MAKER: [{string.Join(", ", forth.Stack)}]");
        
        // Should have incremented value on stack
        Assert.Single(forth.Stack);
        Assert.Equal(1L, forth.Stack[0]);
        
        // Verify FIRST-WORD was created
        var words = forth.GetAllWordNames();
        Assert.Contains("FIRST-WORD", words, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateDoesInsideColonDefinition_ShouldWork()
    {
        var forth = new ForthInterpreter();
        
        // Full ERROR-COUNT pattern
        await forth.EvalAsync(": ERROR-COUNT CREATE DUP , CELL+ DOES> @ ;");
        
        _output.WriteLine("ERROR-COUNT defined");
        
        // Use it
        await forth.EvalAsync("0 ERROR-COUNT CORE-ERRORS");
        
        _output.WriteLine($"Stack after ERROR-COUNT: [{string.Join(", ", forth.Stack)}]");
        
        // Should have incremented value on stack
        Assert.Single(forth.Stack);
        Assert.Equal(1L, forth.Stack[0]);
        
        // Verify CORE-ERRORS was created
        var words = forth.GetAllWordNames();
        Assert.Contains("CORE-ERRORS", words, StringComparer.OrdinalIgnoreCase);
        
        // Execute CORE-ERRORS - should push the offset (0)
        await forth.EvalAsync("SP!");  // Clear stack
        await forth.EvalAsync("CORE-ERRORS");
        
        _output.WriteLine($"Stack after CORE-ERRORS: [{string.Join(", ", forth.Stack)}]");
        
        Assert.Single(forth.Stack);
        Assert.Equal(0L, forth.Stack[0]);
    }

    [Fact]
    public async Task MultipleErrorCountCalls_ShouldAccumulateOffset()
    {
        var forth = new ForthInterpreter();
        
        await forth.EvalAsync(": ERROR-COUNT CREATE DUP , CELL+ DOES> @ ;");
        
        // Chain multiple calls
        await forth.EvalAsync("0 ERROR-COUNT E1 ERROR-COUNT E2 ERROR-COUNT E3");
        
        _output.WriteLine($"Stack after chained calls: [{string.Join(", ", forth.Stack)}]");
        
        // Should have accumulated offset
        Assert.Single(forth.Stack);
        Assert.Equal(3L, forth.Stack[0]);
        
        // Verify all words created
        var words = forth.GetAllWordNames();
        Assert.Contains("E1", words, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("E2", words, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("E3", words, StringComparer.OrdinalIgnoreCase);
        
        // Verify each word returns its offset
        await forth.EvalAsync("SP! E1");
        Assert.Equal(0L, forth.Stack[0]);
        
        await forth.EvalAsync("SP! E2");
        Assert.Equal(1L, forth.Stack[0]);
        
        await forth.EvalAsync("SP! E3");
        Assert.Equal(2L, forth.Stack[0]);
    }

    [Fact]
    public async Task CreateInInterpretMode_ShouldNotPushAddress()
    {
        var forth = new ForthInterpreter();
        
        // CREATE in interpret mode should NOT push address
        await forth.EvalAsync("CREATE TEST-VAR");
        
        _output.WriteLine($"Stack after CREATE: [{string.Join(", ", forth.Stack)}]");
        
        // Stack should be empty
        Assert.Empty(forth.Stack);
        
        // But the word should exist
        var words = forth.GetAllWordNames();
        Assert.Contains("TEST-VAR", words, StringComparer.OrdinalIgnoreCase);
        
        // Executing it SHOULD push address
        await forth.EvalAsync("TEST-VAR");
        Assert.Single(forth.Stack);
        Assert.IsType<long>(forth.Stack[0]);
    }

    [Fact]
    public async Task CreateFollowedByDupAllot_ShouldWork()
    {
        var forth = new ForthInterpreter();
        
        // Put a value on stack
        await forth.EvalAsync("10");
        
        _output.WriteLine($"Stack before CREATE: [{string.Join(", ", forth.Stack)}]");
        
        // CREATE should leave it there
        await forth.EvalAsync("CREATE ARRAY");
        
        _output.WriteLine($"Stack after CREATE: [{string.Join(", ", forth.Stack)}]");
        
        Assert.Single(forth.Stack);
        Assert.Equal(10L, forth.Stack[0]);
        
        // DUP should duplicate it
        await forth.EvalAsync("DUP");
        
        _output.WriteLine($"Stack after DUP: [{string.Join(", ", forth.Stack)}]");
        
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(10L, forth.Stack[0]);
        Assert.Equal(10L, forth.Stack[1]);
        
        // ALLOT should consume one copy
        await forth.EvalAsync("ALLOT");
        
        _output.WriteLine($"Stack after ALLOT: [{string.Join(", ", forth.Stack)}]");
        
        Assert.Single(forth.Stack);
        Assert.Equal(10L, forth.Stack[0]);
    }

    [Fact]
    public async Task ErrorReportPattern_Line33_ShouldWork()
    {
        var forth = new ForthInterpreter();
        
        // Define ERROR-COUNT
        await forth.EvalAsync(": ERROR-COUNT CREATE DUP , CELL+ DOES> @ ;");
        
        // Accumulate offset like errorreport.fth does
        await forth.EvalAsync("0 ERROR-COUNT E1 ERROR-COUNT E2");
        
        _output.WriteLine($"Stack before line 33: [{string.Join(", ", forth.Stack)}]");
        
        var stackBefore = forth.Stack.Count;
        var valueBefore = stackBefore > 0 ? forth.Stack[0] : null;
        
        // This is the pattern from line 33
        await forth.EvalAsync("CREATE ERRORS[] DUP ALLOT CONSTANT #ERROR-COUNTS");
        
        _output.WriteLine($"Stack after line 33: [{string.Join(", ", forth.Stack)}]");
        
        // Verify ERRORS[] was created
        var words = forth.GetAllWordNames();
        Assert.Contains("ERRORS[]", words, StringComparer.OrdinalIgnoreCase);
        
        // Verify #ERROR-COUNTS was created and has correct value
        Assert.Contains("#ERROR-COUNTS", words, StringComparer.OrdinalIgnoreCase);
        
        await forth.EvalAsync("SP! #ERROR-COUNTS");
        Assert.Single(forth.Stack);
        Assert.Equal(2L, forth.Stack[0]);
    }

    [Fact]
    public async Task NestedColonDefinitions_ShouldFail()
    {
        var forth = new ForthInterpreter();
        
        // Start outer definition
        await forth.EvalAsync(": OUTER");
        
        // Try to start nested definition - should fail
        var ex = await Assert.ThrowsAsync<ForthException>(async () =>
        {
            await forth.EvalAsync(": INNER");
        });
        
        _output.WriteLine($"Expected error: {ex.Message}");
        
        Assert.Contains("nested", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ColonNonameInsideColon_ShouldWork()
    {
        var forth = new ForthInterpreter();
        
        // :NONAME should work inside : because it creates an anonymous word
        var result = await forth.EvalAsync(": OUTER :NONAME 42 ; EXECUTE ; OUTER");
        
        _output.WriteLine($"Result: {result}");
        _output.WriteLine($"Stack: [{string.Join(", ", forth.Stack)}]");
        
        // Should execute the anonymous word
        Assert.Single(forth.Stack);
        Assert.Equal(42L, forth.Stack[0]);
    }
}
