using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core.ControlFlow;

/// <summary>
/// Regression tests for bracket conditional state management fix (2025-01-15).
/// 
/// Issue: [IF] was consuming stack values incorrectly and losing state across line boundaries.
/// Root Cause: _bracketIfActiveDepth wasn't being maintained properly for multi-line evaluation.
/// 
/// Fix: Modified [IF]/[ELSE]/[THEN] to properly track depth across EvalAsync calls:
/// - [IF] ALWAYS increments _bracketIfActiveDepth regardless of condition
/// - [ELSE] uses lenient check (only error if depth=0 AND not skipping)
/// - [THEN] safely decrements with bounds checking
/// </summary>
public class BracketIfStateManagementTests
{
    [Fact]
    public async Task BracketIF_DoesNotConsumeUnrelatedStackValue()
    {
        // Regression test for: BASE @ value being consumed by [IF] block
        var forth = new ForthInterpreter();
        
        // Pattern from ttester.4th that was failing:
        // BASE @ on stack, then [IF] block that shouldn't consume it
        await forth.EvalAsync("BASE @");
        Assert.Single(forth.Stack);
        Assert.Equal(10L, forth.Stack[0]); // Decimal base
        
        // This [IF] should NOT consume the BASE value
        await forth.EvalAsync(@"
            -1 [IF]
                42 CONSTANT TEST-CONST
            [THEN]
        ");
        
        // BASE value should still be on stack
        Assert.Single(forth.Stack);
        Assert.Equal(10L, forth.Stack[0]);
        
        // And constant should be defined
        await forth.EvalAsync("TEST-CONST");
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(10L, forth.Stack[0]); // BASE still there
        Assert.Equal(42L, forth.Stack[1]); // TEST-CONST pushed
    }
    
    [Fact]
    public async Task BracketIF_MultiLine_MaintainsStateAcrossLines()
    {
        // Regression test for: State loss when [IF] and [THEN] on different lines
        var forth = new ForthInterpreter();
        
        // Load line by line (simulates file loading with line-by-line eval)
        await forth.EvalAsync("-1");  // Push TRUE flag
        Assert.Single(forth.Stack);
        
        await forth.EvalAsync("[IF]");  // Start conditional, consume flag
        Assert.Empty(forth.Stack);
        
        await forth.EvalAsync("99 CONSTANT VALUE1");  // Define inside [IF]
        Assert.Empty(forth.Stack);
        
        await forth.EvalAsync("[THEN]");  // End conditional
        Assert.Empty(forth.Stack);
        
        // Verify constant was defined
        await forth.EvalAsync("VALUE1");
        Assert.Single(forth.Stack);
        Assert.Equal(99L, forth.Stack[0]);
    }
    
    [Fact]
    public async Task BracketIF_NestedMultiLine_MaintainsCorrectDepth()
    {
        // Regression test for: Nested [IF] blocks losing depth tracking
        var forth = new ForthInterpreter();
        
        // Pattern from ttester.4th with nested [IF]:
        // "FLOATING" ENVIRONMENT? [IF]  \ Line 1: pushes -1, -1; [IF] consumes one
        //     [IF]                       \ Line 2: consumes remaining -1
        //         TRUE                   \ Line 3: pushes -1
        //     [ELSE]                     \ Line 4: should work (depth=2 here)
        //         FALSE
        //     [THEN]                     \ Line 5: depth back to 1
        // [ELSE]
        //     FALSE
        // [THEN]                         \ Line 6: depth back to 0
        
        await forth.EvalAsync(@"
            ""FLOATING"" ENVIRONMENT? [IF]
                [IF]
                    -1
                [ELSE]
                    0
                [THEN]
            [ELSE]
                0
            [THEN]
            CONSTANT TEST-FLAG
        ");
        
        // Should have defined TEST-FLAG with value -1 (TRUE)
        await forth.EvalAsync("TEST-FLAG");
        Assert.Single(forth.Stack);
        Assert.Equal(-1L, forth.Stack[0]);
    }
    
    [Fact]
    public async Task BracketIF_FalseCondition_SkipsToThen()
    {
        // Verify that false [IF] properly skips to [THEN]
        var forth = new ForthInterpreter();
        
        await forth.EvalAsync(@"
            0 [IF]
                999 CONSTANT SHOULD-NOT-DEFINE
            [THEN]
        ");
        
        // SHOULD-NOT-DEFINE should not exist
        await Assert.ThrowsAsync<ForthException>(async () =>
        {
            await forth.EvalAsync("SHOULD-NOT-DEFINE");
        });
    }
    
    [Fact]
    public async Task BracketIF_FalseCondition_ExecutesElse()
    {
        // Verify that false [IF] properly executes [ELSE] branch
        var forth = new ForthInterpreter();
        
        await forth.EvalAsync(@"
            0 [IF]
                111 CONSTANT WRONG-VALUE
            [ELSE]
                222 CONSTANT CORRECT-VALUE
            [THEN]
        ");
        
        // WRONG-VALUE should not exist
        await Assert.ThrowsAsync<ForthException>(async () =>
        {
            await forth.EvalAsync("WRONG-VALUE");
        });
        
        // CORRECT-VALUE should exist with value 222
        await forth.EvalAsync("CORRECT-VALUE");
        Assert.Single(forth.Stack);
        Assert.Equal(222L, forth.Stack[0]);
    }
    
    [Fact]
    public async Task BracketIF_TrueCondition_SkipsElse()
    {
        // Verify that true [IF] properly skips [ELSE] branch
        var forth = new ForthInterpreter();
        
        await forth.EvalAsync(@"
            -1 [IF]
                333 CONSTANT CORRECT-VALUE
            [ELSE]
                444 CONSTANT WRONG-VALUE
            [THEN]
        ");
        
        // CORRECT-VALUE should exist
        await forth.EvalAsync("CORRECT-VALUE");
        Assert.Single(forth.Stack);
        Assert.Equal(333L, forth.Stack[0]);
        
        // Clear stack by creating new interpreter for next check
        forth = new ForthInterpreter();
        await forth.EvalAsync(@"
            -1 [IF]
                333 CONSTANT CORRECT-VALUE
            [ELSE]
                444 CONSTANT WRONG-VALUE
            [THEN]
        ");
        
        // WRONG-VALUE should not exist
        await Assert.ThrowsAsync<ForthException>(async () =>
        {
            await forth.EvalAsync("WRONG-VALUE");
        });
    }
    
    [Fact]
    public async Task BracketIF_PreservesStackAcrossMultiLineBlocks()
    {
        // Comprehensive test: Stack values should be preserved across multi-line blocks
        var forth = new ForthInterpreter();
        
        // Put something on stack before [IF]
        await forth.EvalAsync("123 456");
        Assert.Equal(2, forth.Stack.Count);
        
        // Multi-line [IF] block that doesn't touch those values
        await forth.EvalAsync("-1");  // Push condition
        Assert.Equal(3, forth.Stack.Count);
        
        await forth.EvalAsync("[IF]");  // Consume condition only
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(123L, forth.Stack[0]);
        Assert.Equal(456L, forth.Stack[1]);
        
        await forth.EvalAsync("789 DROP");  // Some operations inside [IF]
        Assert.Equal(2, forth.Stack.Count);
        
        await forth.EvalAsync("[THEN]");  // End block
        
        // Original stack values should still be there
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(123L, forth.Stack[0]);
        Assert.Equal(456L, forth.Stack[1]);
    }
    
    [Fact]
    public async Task BracketIF_TripleNested_MaintainsCorrectDepth()
    {
        // Stress test: Triple-nested [IF] blocks
        var forth = new ForthInterpreter();
        
        await forth.EvalAsync(@"
            -1 [IF]
                -1 [IF]
                    -1 [IF]
                        100 CONSTANT INNER-VALUE
                    [THEN]
                [THEN]
            [THEN]
        ");
        
        await forth.EvalAsync("INNER-VALUE");
        Assert.Single(forth.Stack);
        Assert.Equal(100L, forth.Stack[0]);
    }
    
    [Fact]
    public async Task BracketIF_MixedTrueFalse_HandlesCorrectly()
    {
        // Test mix of true and false conditions in nested structure
        var forth = new ForthInterpreter();
        
        await forth.EvalAsync(@"
            -1 [IF]          \ TRUE: execute
                0 [IF]       \ FALSE: skip to [ELSE]
                    1111 CONSTANT SKIP1
                [ELSE]       \ Execute this branch
                    2222 CONSTANT KEEP1
                [THEN]
            [ELSE]           \ Skip this (outer was TRUE)
                3333 CONSTANT SKIP2
            [THEN]
        ");
        
        // Only KEEP1 should be defined
        await forth.EvalAsync("KEEP1");
        Assert.Single(forth.Stack);
        Assert.Equal(2222L, forth.Stack[0]);
        
        // SKIP1 and SKIP2 should not exist
        await Assert.ThrowsAsync<ForthException>(async () => await forth.EvalAsync("SKIP1"));
        await Assert.ThrowsAsync<ForthException>(async () => await forth.EvalAsync("SKIP2"));
    }
}
