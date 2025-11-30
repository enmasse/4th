using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using Xunit.Abstractions;

namespace Forth.Tests.Core.Defining;

public class CreateDoesStackTests
{
    private readonly ITestOutputHelper _output;

    public CreateDoesStackTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Create_InInterpretMode_ShouldNotModifyStack()
    {
        var forth = new ForthInterpreter();
        
        // Push test values
        await forth.EvalAsync("1 2 3");
        _output.WriteLine($"Stack before CREATE: [{string.Join(", ", forth.Stack)}]");
        Assert.Equal(3, forth.Stack.Count);
        
        // CREATE should not modify stack
        await forth.EvalAsync("CREATE TEST");
        _output.WriteLine($"Stack after CREATE: [{string.Join(", ", forth.Stack)}]");
        
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(1L, forth.Stack[0]);
        Assert.Equal(2L, forth.Stack[1]);
        Assert.Equal(3L, forth.Stack[2]);
    }

    [Fact]
    public async Task Create_InCompileMode_ShouldNotModifyStack()
    {
        var forth = new ForthInterpreter();
        
        await forth.EvalAsync(": TESTER 42 CREATE FOO 99 ;");
        
        // Execute TESTER
        await forth.EvalAsync("TESTER");
        
        _output.WriteLine($"Stack after TESTER: [{string.Join(", ", forth.Stack)}]");
        
        // Should have pushed 42 before CREATE and 99 after
        // CREATE itself should not affect stack
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(42L, forth.Stack[0]);
        Assert.Equal(99L, forth.Stack[1]);
        
        // FOO should have been created
        var words = forth.GetAllWordNames();
        Assert.Contains("FOO", words, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateDupComma_StackBehavior()
    {
        var forth = new ForthInterpreter();
        
        await forth.EvalAsync(": MAKER CREATE DUP , ;");
        
        // Call with value
        await forth.EvalAsync("100 MAKER VALUE-HOLDER");
        
        _output.WriteLine($"Stack after MAKER: [{string.Join(", ", forth.Stack)}]");
        
        // DUP should have duplicated 100, one copy stored by ,
        // One copy should remain on stack
        Assert.Single(forth.Stack);
        Assert.Equal(100L, forth.Stack[0]);
        
        // VALUE-HOLDER should exist
        var words = forth.GetAllWordNames();
        Assert.Contains("VALUE-HOLDER", words, StringComparer.OrdinalIgnoreCase);
        
        // Check that the value was stored
        await forth.EvalAsync("SP! VALUE-HOLDER @");
        Assert.Single(forth.Stack);
        Assert.Equal(100L, forth.Stack[0]);
    }

    [Fact]
    public async Task Does_WithoutPrecedingCreate_ShouldFail()
    {
        var forth = new ForthInterpreter();
        
        // DOES> without CREATE should fail
        var ex = await Assert.ThrowsAsync<ForthException>(async () =>
        {
            await forth.EvalAsync(": BAD-WORD DOES> @ ;");
            await forth.EvalAsync("BAD-WORD");
        });
        
        _output.WriteLine($"Expected error: {ex.Message}");
    }

    [Fact]
    public async Task CreateDoes_BasicPattern()
    {
        var forth = new ForthInterpreter();
        
        // Simple constant maker
        await forth.EvalAsync(": CONSTANT-MAKER CREATE , DOES> @ ;");
        
        // Create a constant
        await forth.EvalAsync("42 CONSTANT-MAKER MY-CONST");
        
        // Stack should be empty after creation
        Assert.Empty(forth.Stack);
        
        // Execute the constant
        await forth.EvalAsync("MY-CONST");
        
        _output.WriteLine($"Stack after MY-CONST: [{string.Join(", ", forth.Stack)}]");
        
        Assert.Single(forth.Stack);
        Assert.Equal(42L, forth.Stack[0]);
    }

    [Fact]
    public async Task CreateDoes_WithStackManipulation()
    {
        var forth = new ForthInterpreter();
        
        // ERROR-COUNT pattern: keeps offset on stack
        await forth.EvalAsync(": OFFSET-MAKER CREATE DUP , CELL+ DOES> @ ;");
        
        // Start with offset 0
        await forth.EvalAsync("0 OFFSET-MAKER FIRST");
        
        _output.WriteLine($"Stack after FIRST: [{string.Join(", ", forth.Stack)}]");
        
        // Should have offset 1 on stack
        Assert.Single(forth.Stack);
        var offset1 = forth.Stack[0];
        Assert.Equal(1L, offset1);
        
        // Create second with accumulated offset
        await forth.EvalAsync("OFFSET-MAKER SECOND");
        
        _output.WriteLine($"Stack after SECOND: [{string.Join(", ", forth.Stack)}]");
        
        // Should have offset 2 on stack
        Assert.Single(forth.Stack);
        Assert.Equal(2L, forth.Stack[0]);
        
        // Verify FIRST returns 0
        await forth.EvalAsync("SP! FIRST");
        Assert.Equal(0L, forth.Stack[0]);
        
        // Verify SECOND returns 1
        await forth.EvalAsync("SP! SECOND");
        Assert.Equal(1L, forth.Stack[0]);
    }

    [Fact]
    public async Task CreateInColonWithStackValues_ShouldPreserveStack()
    {
        var forth = new ForthInterpreter();
        
        // Definition that puts value before and after CREATE
        await forth.EvalAsync(": TESTER 10 20 CREATE DUMMY 30 ;");
        
        await forth.EvalAsync("TESTER");
        
        _output.WriteLine($"Stack: [{string.Join(", ", forth.Stack)}]");
        
        // Should have all three values
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(10L, forth.Stack[0]);
        Assert.Equal(20L, forth.Stack[1]);
        Assert.Equal(30L, forth.Stack[2]);
    }

    [Fact]
    public async Task DoesBody_ShouldAccessDataField()
    {
        var forth = new ForthInterpreter();
        
        // Array maker: size CREATE name DOES> pushes base address
        await forth.EvalAsync(": ARRAY CREATE CELLS ALLOT DOES> ;");
        
        // Create 10-cell array
        await forth.EvalAsync("10 ARRAY MY-ARRAY");
        
        // Execute should push address
        await forth.EvalAsync("MY-ARRAY");
        
        _output.WriteLine($"Stack: [{string.Join(", ", forth.Stack)}]");
        
        Assert.Single(forth.Stack);
        Assert.IsType<long>(forth.Stack[0]);
        
        var addr = (long)forth.Stack[0];
        
        // Store and retrieve value
        await forth.EvalAsync($"SP! 42 {addr} !");
        await forth.EvalAsync($"{addr} @");
        
        Assert.Single(forth.Stack);
        Assert.Equal(42L, forth.Stack[0]);
    }

    [Fact]
    public async Task MultipleDoesWords_ShouldBeIndependent()
    {
        var forth = new ForthInterpreter();
        
        await forth.EvalAsync(": CONST CREATE , DOES> @ ;");
        
        // Create multiple constants
        await forth.EvalAsync("10 CONST TEN");
        await forth.EvalAsync("20 CONST TWENTY");
        await forth.EvalAsync("30 CONST THIRTY");
        
        // Each should return its own value
        await forth.EvalAsync("SP! TEN");
        Assert.Equal(10L, forth.Stack[0]);
        
        await forth.EvalAsync("SP! TWENTY");
        Assert.Equal(20L, forth.Stack[0]);
        
        await forth.EvalAsync("SP! THIRTY");
        Assert.Equal(30L, forth.Stack[0]);
    }

    [Fact]
    public async Task DoesWithComplexBody_ShouldWork()
    {
        var forth = new ForthInterpreter();
        
        // Counter: CREATE name DOES> DUP @ 1+ TUCK SWAP !
        await forth.EvalAsync(": COUNTER CREATE 0 , DOES> DUP @ 1+ TUCK SWAP ! ;");
        
        await forth.EvalAsync("COUNTER CTR");
        
        // Each execution should increment and return new value
        await forth.EvalAsync("CTR");
        _output.WriteLine($"First call: {forth.Stack[0]}");
        Assert.Equal(1L, forth.Stack[0]);
        
        await forth.EvalAsync("SP! CTR");
        _output.WriteLine($"Second call: {forth.Stack[0]}");
        Assert.Equal(2L, forth.Stack[0]);
        
        await forth.EvalAsync("SP! CTR");
        _output.WriteLine($"Third call: {forth.Stack[0]}");
        Assert.Equal(3L, forth.Stack[0]);
    }
}
