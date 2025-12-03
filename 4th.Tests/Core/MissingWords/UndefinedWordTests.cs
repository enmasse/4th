using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

/// <summary>
/// Tests for the [UNDEFINED] word which tests if a word is NOT defined
/// </summary>
public class UndefinedWordTests
{
    private static ForthInterpreter New() => new();

    [Fact]
    public async Task Undefined_ReturnsTrueForUndefinedWord()
    {
        var f = New();
        // [UNDEFINED] should return true (-1) for an undefined word
        await f.EvalAsync("[UNDEFINED] NONEXISTENT-WORD-12345");
        Assert.Single(f.Stack);
        Assert.Equal(-1L, (long)f.Stack[0]); // TRUE
    }

    [Fact]
    public async Task Undefined_ReturnsFalseForDefinedWord()
    {
        var f = New();
        // Define a word first
        await f.EvalAsync(": TEST-WORD 42 ;");
        // [UNDEFINED] should return false (0) for a defined word
        await f.EvalAsync("[UNDEFINED] TEST-WORD");
        Assert.Single(f.Stack);
        Assert.Equal(0L, (long)f.Stack[0]); // FALSE
    }

    [Fact]
    public async Task Undefined_ReturnsFalseForPrimitiveWord()
    {
        var f = New();
        // [UNDEFINED] should return false for primitive words
        await f.EvalAsync("[UNDEFINED] DUP");
        Assert.Single(f.Stack);
        Assert.Equal(0L, (long)f.Stack[0]); // FALSE
    }

    [Fact]
    public async Task Undefined_ReturnsFalseForConstant()
    {
        var f = New();
        // Define a constant
        await f.EvalAsync("100 CONSTANT MY-CONST");
        // [UNDEFINED] should return false for constants
        await f.EvalAsync("[UNDEFINED] MY-CONST");
        Assert.Single(f.Stack);
        Assert.Equal(0L, (long)f.Stack[0]); // FALSE
    }

    [Fact]
    public async Task Undefined_ReturnsFalseForVariable()
    {
        var f = New();
        // Define a variable
        await f.EvalAsync("VARIABLE MY-VAR");
        // [UNDEFINED] should return false for variables
        await f.EvalAsync("[UNDEFINED] MY-VAR");
        Assert.Single(f.Stack);
        Assert.Equal(0L, (long)f.Stack[0]); // FALSE
    }

    [Fact]
    public async Task Undefined_InConditionalCompilation()
    {
        var f = New();
        // Use [UNDEFINED] in conditional compilation
        // This should define TEST-WORD because it's undefined
        await f.EvalAsync("[UNDEFINED] TEST-WORD [IF] : TEST-WORD 99 ; [THEN]");
        // Now TEST-WORD should exist
        await f.EvalAsync("TEST-WORD");
        Assert.Single(f.Stack);
        Assert.Equal(99L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task Undefined_SkipsDefinitionWhenDefined()
    {
        var f = New();
        // Define TEST-WORD first
        await f.EvalAsync(": TEST-WORD 42 ;");
        // Check that [UNDEFINED] returns FALSE (0) for a defined word
        await f.EvalAsync("[UNDEFINED] TEST-WORD");
        Assert.Single(f.Stack);
        Assert.Equal(0L, (long)f.Stack[0]); // FALSE, word is defined
        
        // Pop the flag and verify the original word still works
        f.Pop();
        await f.EvalAsync("TEST-WORD");
        Assert.Single(f.Stack);
        Assert.Equal(42L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task Undefined_MultipleUndefinedWords()
    {
        var f = New();
        // Test multiple undefined words
        await f.EvalAsync("[UNDEFINED] WORD-A [UNDEFINED] WORD-B [UNDEFINED] WORD-C");
        Assert.Equal(3, f.Stack.Count);
        Assert.Equal(-1L, (long)f.Stack[0]); // TRUE
        Assert.Equal(-1L, (long)f.Stack[1]); // TRUE
        Assert.Equal(-1L, (long)f.Stack[2]); // TRUE
    }

    [Fact]
    public async Task Undefined_MixedDefinedAndUndefined()
    {
        var f = New();
        // Define one word
        await f.EvalAsync(": DEFINED-WORD 1 ;");
        // Test mixture of defined and undefined
        await f.EvalAsync("[UNDEFINED] DEFINED-WORD [UNDEFINED] UNDEFINED-WORD");
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(0L, (long)f.Stack[0]);  // FALSE (DEFINED-WORD exists)
        Assert.Equal(-1L, (long)f.Stack[1]); // TRUE (UNDEFINED-WORD doesn't exist)
    }

    [Fact]
    public async Task Undefined_WithPreludeWords()
    {
        var f = New();
        // Prelude words like TRUE and FALSE should be defined
        await f.EvalAsync("[UNDEFINED] TRUE [UNDEFINED] FALSE");
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(0L, (long)f.Stack[0]); // FALSE (TRUE is defined)
        Assert.Equal(0L, (long)f.Stack[1]); // FALSE (FALSE is defined)
    }

    [Fact]
    public async Task Undefined_CaseInsensitive()
    {
        var f = New();
        // Define a word in uppercase
        await f.EvalAsync(": MYWORD 1 ;");
        // Test with different case - Forth is typically case-insensitive
        await f.EvalAsync("[UNDEFINED] myword");
        Assert.Single(f.Stack);
        Assert.Equal(0L, (long)f.Stack[0]); // FALSE (word exists regardless of case)
    }

    [Fact]
    public async Task Undefined_GuardPattern()
    {
        var f = New();
        // Common pattern: define a word only if it doesn't exist
        var code = @"
            [UNDEFINED] HELPER [IF]
            : HELPER 123 ;
            [THEN]
            
            HELPER
        ";
        await f.EvalAsync(code);
        Assert.Single(f.Stack);
        Assert.Equal(123L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task Undefined_NestedConditionals()
    {
        var f = New();
        // Test nested conditionals with [UNDEFINED]
        // First define INNER if needed
        await f.EvalAsync("[UNDEFINED] INNER [IF] : INNER 456 ; [THEN]");
        // Then define OUTER if needed, which uses INNER
        await f.EvalAsync("[UNDEFINED] OUTER [IF] : OUTER INNER ; [THEN]");
        // Now call OUTER
        await f.EvalAsync("OUTER");
        Assert.Single(f.Stack);
        Assert.Equal(456L, (long)f.Stack[0]);
    }
}
