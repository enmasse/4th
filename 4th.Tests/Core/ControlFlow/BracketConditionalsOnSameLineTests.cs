using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core.ControlFlow;

/// <summary>
/// Tests for bracketed conditionals ([IF] [ELSE] [THEN]) when they appear on the same line.
/// These tests verify that the bracket conditional handling works correctly with single-line definitions.
/// </summary>
public class BracketConditionalsOnSameLineTests
{
    private static ForthInterpreter New() => new();

    [Fact]
    public async Task BracketIF_OnSameLine_True_ExecutesThenPart()
    {
        var f = New();
        // When condition is true, execute the THEN part
        await f.EvalAsync("-1 [IF] 42 [THEN]");
        Assert.Single(f.Stack);
        Assert.Equal(42L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task BracketIF_OnSameLine_False_SkipsThenPart()
    {
        var f = New();
        // When condition is false, skip the THEN part
        await f.EvalAsync("0 [IF] 42 [THEN]");
        Assert.Empty(f.Stack);
    }

    [Fact]
    public async Task BracketIF_OnSameLine_WithElse_True_ExecutesThenPart()
    {
        var f = New();
        // When condition is true, execute THEN part, skip ELSE part
        await f.EvalAsync("-1 [IF] 42 [ELSE] 99 [THEN]");
        Assert.Single(f.Stack);
        Assert.Equal(42L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task BracketIF_OnSameLine_WithElse_False_ExecutesElsePart()
    {
        var f = New();
        // When condition is false, skip THEN part, execute ELSE part
        await f.EvalAsync("0 [IF] 42 [ELSE] 99 [THEN]");
        Assert.Single(f.Stack);
        Assert.Equal(99L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task BracketIF_OnSameLine_NestedConditionals()
    {
        var f = New();
        // Nested [IF] on same line
        await f.EvalAsync("-1 [IF] -1 [IF] 123 [THEN] [THEN]");
        Assert.Single(f.Stack);
        Assert.Equal(123L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task BracketIF_OnSameLine_NestedConditionals_OuterFalse()
    {
        var f = New();
        // Outer condition false, inner should be skipped entirely
        await f.EvalAsync("0 [IF] -1 [IF] 123 [THEN] [THEN]");
        Assert.Empty(f.Stack);
    }

    [Fact]
    public async Task BracketIF_OnSameLine_NestedConditionals_InnerFalse()
    {
        var f = New();
        // Outer condition true, inner condition false
        await f.EvalAsync("-1 [IF] 0 [IF] 123 [THEN] [THEN]");
        Assert.Empty(f.Stack);
    }

    [Fact]
    public async Task BracketIF_OnSameLine_MultipleStatements()
    {
        var f = New();
        // Multiple statements in THEN part
        await f.EvalAsync("-1 [IF] 10 20 30 [THEN]");
        Assert.Equal(3, f.Stack.Count);
        Assert.Equal(10L, (long)f.Stack[0]);
        Assert.Equal(20L, (long)f.Stack[1]);
        Assert.Equal(30L, (long)f.Stack[2]);
    }

    [Fact]
    public async Task BracketIF_OnSameLine_WithUndefined_True()
    {
        var f = New();
        // [UNDEFINED] returns true for undefined words, so [IF] should execute
        await f.EvalAsync("[UNDEFINED] NONEXISTENT [IF] 555 [THEN]");
        Assert.Single(f.Stack);
        Assert.Equal(555L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task BracketIF_OnSameLine_WithUndefined_False()
    {
        var f = New();
        // Define a word first
        await f.EvalAsync(": TEST-WORD 1 ;");
        // [UNDEFINED] returns false for defined words, so [IF] should skip
        await f.EvalAsync("[UNDEFINED] TEST-WORD [IF] 555 [THEN]");
        Assert.Empty(f.Stack);
    }

    [Fact]
    public async Task BracketIF_OnSameLine_WithArithmetic()
    {
        var f = New();
        // Use arithmetic result as condition
        await f.EvalAsync("5 3 > [IF] 777 [THEN]");
        Assert.Single(f.Stack);
        Assert.Equal(777L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task BracketIF_OnSameLine_ComplexNesting()
    {
        var f = New();
        // Complex nested conditionals with ELSE
        await f.EvalAsync("-1 [IF] 0 [IF] 111 [ELSE] 222 [THEN] [ELSE] 333 [THEN]");
        Assert.Single(f.Stack);
        Assert.Equal(222L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task BracketIF_OnSameLine_BeforeColonDefinition()
    {
        var f = New();
        // Use [IF] to conditionally define a word
        await f.EvalAsync("[UNDEFINED] MYWORD [IF] : MYWORD 888 ; [THEN]");
        await f.EvalAsync("MYWORD");
        Assert.Single(f.Stack);
        Assert.Equal(888L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task BracketIF_OnSameLine_SkipColonDefinition()
    {
        var f = New();
        // Define word first
        await f.EvalAsync(": MYWORD 100 ;");
        // Verify [UNDEFINED] returns false (0) for a defined word
        await f.EvalAsync("[UNDEFINED] MYWORD");
        Assert.Single(f.Stack);
        Assert.Equal(0L, (long)f.Stack[0]); // FALSE, word is defined
        
        // Pop the flag and verify the original word still works
        f.Pop();
        await f.EvalAsync("MYWORD");
        Assert.Single(f.Stack);
        Assert.Equal(100L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task BracketIF_OnSameLine_WithVariableDefinition()
    {
        var f = New();
        // Conditionally define a variable
        await f.EvalAsync("[UNDEFINED] MYVAR [IF] VARIABLE MYVAR [THEN]");
        // Verify variable exists
        await f.EvalAsync("123 MYVAR !");
        await f.EvalAsync("MYVAR @");
        Assert.Single(f.Stack);
        Assert.Equal(123L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task BracketIF_OnSameLine_WithConstantDefinition()
    {
        var f = New();
        // Conditionally define a constant
        await f.EvalAsync("[UNDEFINED] MYCONST [IF] 999 CONSTANT MYCONST [THEN]");
        await f.EvalAsync("MYCONST");
        Assert.Single(f.Stack);
        Assert.Equal(999L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task BracketIF_OnSameLine_EmptyThenBlock()
    {
        var f = New();
        // Empty THEN block should be allowed
        await f.EvalAsync("-1 [IF] [THEN]");
        Assert.Empty(f.Stack);
    }

    [Fact]
    public async Task BracketIF_OnSameLine_EmptyElseBlock()
    {
        var f = New();
        // Empty ELSE block should be allowed
        await f.EvalAsync("0 [IF] 42 [ELSE] [THEN]");
        Assert.Empty(f.Stack);
    }

    [Fact]
    public async Task BracketIF_OnSameLine_MultipleConditionalsSequential()
    {
        var f = New();
        // Multiple sequential conditionals on same line
        await f.EvalAsync("-1 [IF] 1 [THEN] -1 [IF] 2 [THEN] -1 [IF] 3 [THEN]");
        Assert.Equal(3, f.Stack.Count);
        Assert.Equal(1L, (long)f.Stack[0]);
        Assert.Equal(2L, (long)f.Stack[1]);
        Assert.Equal(3L, (long)f.Stack[2]);
    }

    [Fact]
    public async Task BracketIF_OnSameLine_WithStackManipulation()
    {
        var f = New();
        // Use stack manipulation inside conditionals
        await f.EvalAsync("10 20 -1 [IF] + [ELSE] - [THEN]");
        Assert.Single(f.Stack);
        Assert.Equal(30L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task BracketIF_OnSameLine_WithStackManipulation_False()
    {
        var f = New();
        // Use stack manipulation inside conditionals (ELSE path)
        await f.EvalAsync("20 10 0 [IF] + [ELSE] - [THEN]");
        Assert.Single(f.Stack);
        Assert.Equal(10L, (long)f.Stack[0]);
    }
}
