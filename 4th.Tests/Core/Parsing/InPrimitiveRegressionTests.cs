using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core.Parsing;

/// <summary>
/// Regression tests for the >IN primitive.
/// >IN ( -- addr ) returns the address of the input buffer parse position.
/// 
/// These tests verify that >IN can be read and written to control parse position,
/// which is critical for ANS Forth compliance and enables advanced parsing patterns.
/// </summary>
public class InPrimitiveRegressionTests
{
    [Fact]
    public async Task In_ReturnsAddress()
    {
        var forth = new ForthInterpreter();
        // >IN should return an address (the InAddr constant)
        Assert.True(await forth.EvalAsync(">IN"));
        Assert.Single(forth.Stack);
        var addr = (long)forth.Pop();
        Assert.True(addr > 0);
        Assert.Equal(forth.InAddr, addr);
    }

    [Fact]
    public async Task In_InitialValueIsZero()
    {
        var forth = new ForthInterpreter();
        // At start of input, >IN @ should be 0
        Assert.True(await forth.EvalAsync(">IN @"));
        Assert.Single(forth.Stack);
        var value = (long)forth.Pop();
        Assert.Equal(0L, value);
    }

    [Fact]
    public async Task In_AdvancesAfterParsing()
    {
        var forth = new ForthInterpreter();
        // After parsing some tokens, >IN should advance
        Assert.True(await forth.EvalAsync("123 >IN @"));
        Assert.Equal(2, forth.Stack.Count);
        var pos = (long)forth.Pop();
        var value = (long)forth.Pop();
        Assert.Equal(123L, value);
        Assert.True(pos > 0);
    }

    [Fact]
    public async Task In_CanBeWritten()
    {
        var forth = new ForthInterpreter();
        // We can set >IN to skip parts of input
        Assert.True(await forth.EvalAsync(": TEST 5 >IN ! ; TEST hello world"));
        // The word "TEST" executes, sets >IN to 5, which skips "hello" and continues with "world"
        // Since we set >IN to 5, it should point just past "TEST " (5 chars)
        Assert.Empty(forth.Stack);
    }

    [Fact]
    public async Task In_SetToEndOfLine()
    {
        var forth = new ForthInterpreter();
        // Setting >IN to SOURCE's length should skip to end of input
        Assert.True(await forth.EvalAsync("SOURCE NIP >IN ! 123 456"));
        // 123 and 456 should be skipped
        Assert.Empty(forth.Stack);
    }

    [Fact]
    public async Task In_ResetsOnNewLine()
    {
        var forth = new ForthInterpreter();
        // >IN should reset to 0 for each new line
        Assert.True(await forth.EvalAsync("123"));
        Assert.True(await forth.EvalAsync(">IN @"));
        Assert.Single(forth.Stack);
        var value = (long)forth.Pop();
        Assert.Equal(0L, value); // New line starts at 0
    }

    [Fact(Skip = "Advanced >IN manipulation within same line not fully supported - requires character-based parsing")]
    public async Task In_WithWord()
    {
        var forth = new ForthInterpreter();
        // WORD should advance >IN
        Assert.True(await forth.EvalAsync(": TEST >IN @ SWAP >IN @ ; TEST 32 WORD hello"));
        Assert.Equal(3, forth.Stack.Count);
        
        var posAfter = (long)forth.Pop();
        var addr = forth.Pop(); // word address
        var posBefore = (long)forth.Pop();
        
        // posAfter should be > posBefore because WORD consumed input
        Assert.True(posAfter > posBefore);
    }

    [Fact(Skip = "Advanced >IN manipulation within same line not fully supported - requires character-based parsing")]
    public async Task In_Rescan_Pattern()
    {
        var forth = new ForthInterpreter();
        // ANS Forth pattern: rescan by setting >IN back to 0
        Assert.True(await forth.EvalAsync(@"
            VARIABLE SCANS
            : RESCAN? -1 SCANS +! SCANS @ IF 0 >IN ! THEN ;
            2 SCANS ! 345 RESCAN?
        "));
        // Should see 345 three times (initial + 2 rescans)
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(345L, (long)forth.Stack[0]);
        Assert.Equal(345L, (long)forth.Stack[1]);
        Assert.Equal(345L, (long)forth.Stack[2]);
    }

    [Fact(Skip = "Advanced >IN manipulation within same line not fully supported - requires character-based parsing")]
    public async Task In_SkipRestOfLine()
    {
        var forth = new ForthInterpreter();
        // Pattern: skip rest of line by setting >IN to source length
        Assert.True(await forth.EvalAsync(": SKIP SOURCE >IN ! DROP ; SKIP 1 2 3"));
        // 1 2 3 should not be executed
        Assert.Empty(forth.Stack);
    }

    [Fact(Skip = ">IN @ with EVALUATE requires proper source stack management")]
    public async Task In_WithEvaluate()
    {
        var forth = new ForthInterpreter();
        // >IN should work correctly with EVALUATE
        Assert.True(await forth.EvalAsync("S\" 123 456\" EVALUATE >IN @"));
        Assert.Equal(3, forth.Stack.Count);
        Assert.Equal(123L, (long)forth.Stack[0]);
        Assert.Equal(456L, (long)forth.Stack[1]);
        var pos = (long)forth.Stack[2];
        Assert.Equal(0L, pos); // After EVALUATE, back to outer source
    }

    [Fact(Skip = ">IN @ persistence across words requires character-based tracking")]
    public async Task In_Persistence_AcrossWords()
    {
        var forth = new ForthInterpreter();
        // >IN should persist across word calls in same input line
        Assert.True(await forth.EvalAsync(": GET-POS >IN @ ; 1 2 GET-POS 3 4 GET-POS"));
        Assert.Equal(6, forth.Stack.Count);
        Assert.Equal(1L, (long)forth.Stack[0]);
        Assert.Equal(2L, (long)forth.Stack[1]);
        var pos1 = (long)forth.Stack[2];
        Assert.Equal(3L, (long)forth.Stack[3]);
        Assert.Equal(4L, (long)forth.Stack[4]);
        var pos2 = (long)forth.Stack[5];
        
        // pos2 should be > pos1 because we parsed more tokens
        Assert.True(pos2 > pos1);
    }

    [Fact(Skip = "Advanced >IN manipulation with SAVE-INPUT/RESTORE-INPUT not fully integrated")]
    public async Task In_WithSaveRestore()
    {
        var forth = new ForthInterpreter();
        // SAVE-INPUT and RESTORE-INPUT should preserve >IN
        Assert.True(await forth.EvalAsync("5 >IN ! SAVE-INPUT"));
        Assert.Equal(5, forth.Stack.Count); // id, inVal, index, source, n
        
        // Now change >IN
        Assert.True(await forth.EvalAsync("10 >IN !"));
        
        // Restore
        Assert.True(await forth.EvalAsync("RESTORE-INPUT"));
        Assert.Single(forth.Stack);
        Assert.Equal(0L, (long)forth.Pop()); // success flag
        
        // Check >IN was restored to 5
        Assert.True(await forth.EvalAsync(">IN @"));
        Assert.Single(forth.Stack);
        Assert.Equal(5L, (long)forth.Pop());
    }

    [Fact]
    public async Task In_BoundaryCondition_Negative()
    {
        var forth = new ForthInterpreter();
        // Setting >IN to negative should work (used by some test patterns)
        Assert.True(await forth.EvalAsync("-1 >IN !"));
        Assert.True(await forth.EvalAsync(">IN @"));
        Assert.Single(forth.Stack);
        Assert.Equal(-1L, (long)forth.Pop());
    }

    [Fact]
    public async Task In_BoundaryCondition_Large()
    {
        var forth = new ForthInterpreter();
        // Setting >IN to large value should skip all input
        Assert.True(await forth.EvalAsync("9999 >IN ! 123 456"));
        // 123 456 should be skipped
        Assert.Empty(forth.Stack);
    }

    [Fact(Skip = "Advanced >IN manipulation within same line not fully supported - requires character-based parsing")]
    public async Task In_WithSourceAndType()
    {
        var forth = new ForthInterpreter();
        // Pattern from ANS Forth tests: manipulate >IN to skip/show input
        Assert.True(await forth.EvalAsync(@"
            : SHOW-REST SOURCE >IN @ /STRING TYPE ;
        "));
        // This would normally print, but we'll just verify it doesn't crash
        Assert.True(await forth.EvalAsync("SHOW-REST hello world"));
    }

    [Fact(Skip = ">IN @ in colon definitions requires character-based position tracking")]
    public async Task In_WithColon_Definition()
    {
        var forth = new ForthInterpreter();
        // >IN can be used inside colon definitions
        Assert.True(await forth.EvalAsync(": GETPOS >IN @ ;"));
        Assert.True(await forth.EvalAsync("123 GETPOS"));
        Assert.Equal(2, forth.Stack.Count);
        Assert.Equal(123L, (long)forth.Stack[0]);
        var pos = (long)forth.Stack[1];
        Assert.True(pos > 0);
    }
}
