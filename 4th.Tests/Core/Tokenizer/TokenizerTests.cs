using Forth.Core.Interpreter;
using Xunit;
using Xunit.Abstractions;

namespace Forth.Tests.Core.Tokenizer;

public class TokenizerTests
{
    private readonly ITestOutputHelper _output;

    public TokenizerTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [InlineData("ERRORS[]", new[] { "ERRORS[]" })]
    [InlineData("FOO[BAR]", new[] { "FOO[BAR]" })]
    [InlineData("TEST-NAME", new[] { "TEST-NAME" })]
    [InlineData("A B C", new[] { "A", "B", "C" })]
    [InlineData("[ ] IF", new[] { "[", "]", "IF" })]
    [InlineData("WORD1 WORD2", new[] { "WORD1", "WORD2" })]
    public void Tokenizer_ShouldHandleSpecialCharacters(string input, string[] expectedTokens)
    {
        var forth = new ForthInterpreter();
        var tokens = Forth.Core.Interpreter.Tokenizer.Tokenize(input);
        
        _output.WriteLine($"Input: '{input}'");
        _output.WriteLine($"Tokens: [{string.Join(", ", tokens.Select(t => $"'{t}'"))}]");
        _output.WriteLine($"Expected: [{string.Join(", ", expectedTokens.Select(t => $"'{t}'"))}]");
        
        Assert.Equal(expectedTokens.Length, tokens.Count);
        for (int i = 0; i < expectedTokens.Length; i++)
        {
            Assert.Equal(expectedTokens[i], tokens[i]);
        }
    }

    [Fact]
    public void Tokenizer_ShouldHandleBrackets()
    {
        var tokens = Forth.Core.Interpreter.Tokenizer.Tokenize("CREATE ERRORS[] DUP");
        
        _output.WriteLine($"Tokens: [{string.Join(", ", tokens.Select(t => $"'{t}'"))}]");
        
        Assert.Equal(3, tokens.Count);
        Assert.Equal("CREATE", tokens[0]);
        Assert.Equal("ERRORS[]", tokens[1]);
        Assert.Equal("DUP", tokens[2]);
    }

    [Fact]
    public void Tokenizer_ShouldNotSplitWordWithBrackets()
    {
        var input = "VARIABLE ERRORS[]";
        var tokens = Forth.Core.Interpreter.Tokenizer.Tokenize(input);
        
        _output.WriteLine($"Input: '{input}'");
        _output.WriteLine($"Tokens: [{string.Join(", ", tokens.Select(t => $"'{t}'"))}]");
        
        Assert.Equal(2, tokens.Count);
        Assert.Equal("VARIABLE", tokens[0]);
        Assert.Equal("ERRORS[]", tokens[1]);
    }

    [Theory]
    [InlineData("S\" hello\"", new[] { "S\"", "\"hello\"" })]
    [InlineData(".\" test\"", new[] { ".\"", "\"test\"" })]
    [InlineData("ABORT\" error\"", new[] { "ABORT\"", "\"error\"" })]
    public void Tokenizer_ShouldHandleQuotedStrings(string input, string[] expectedTokens)
    {
        var tokens = Forth.Core.Interpreter.Tokenizer.Tokenize(input);
        
        _output.WriteLine($"Input: '{input}'");
        _output.WriteLine($"Tokens: [{string.Join(", ", tokens.Select(t => $"'{t}'"))}]");
        
        Assert.Equal(expectedTokens.Length, tokens.Count);
        for (int i = 0; i < expectedTokens.Length; i++)
        {
            Assert.Equal(expectedTokens[i], tokens[i]);
        }
    }

    [Fact]
    public void Tokenizer_ShouldHandleComments()
    {
        var input = "WORD1 \\ comment\nWORD2";
        var tokens = Forth.Core.Interpreter.Tokenizer.Tokenize(input);
        
        _output.WriteLine($"Input: '{input.Replace("\n", "\\n")}'");
        _output.WriteLine($"Tokens: [{string.Join(", ", tokens.Select(t => $"'{t}'"))}]");
        
        // Comments should be stripped
        Assert.Equal(2, tokens.Count);
        Assert.Equal("WORD1", tokens[0]);
        Assert.Equal("WORD2", tokens[1]);
    }

    [Fact]
    public void Tokenizer_ShouldHandleParenComments()
    {
        var input = "WORD1 ( comment ) WORD2";
        var tokens = Forth.Core.Interpreter.Tokenizer.Tokenize(input);
        
        _output.WriteLine($"Input: '{input}'");
        _output.WriteLine($"Tokens: [{string.Join(", ", tokens.Select(t => $"'{t}'"))}]");
        
        // Paren comments should be stripped
        Assert.Equal(2, tokens.Count);
        Assert.Equal("WORD1", tokens[0]);
        Assert.Equal("WORD2", tokens[1]);
    }

    #region DotParen Tests - Regression tests for .( ... ) immediate printing

    [Fact]
    public void Tokenizer_DotParen_ShouldCreateToken()
    {
        var input = ".( Hello World )";
        var tokens = Forth.Core.Interpreter.Tokenizer.Tokenize(input);
        
        _output.WriteLine($"Input: '{input}'");
        _output.WriteLine($"Tokens: [{string.Join(", ", tokens.Select(t => $"'{t}'"))}]");
        
        // .( should create a token that will be executed during evaluation
        Assert.Single(tokens);
        Assert.Equal(".( Hello World )", tokens[0]);
    }

    [Fact]
    public void Tokenizer_DotParen_WithOtherWords_ShouldCreateTokens()
    {
        var input = "WORD1 .( test message ) WORD2";
        var tokens = Forth.Core.Interpreter.Tokenizer.Tokenize(input);
        
        _output.WriteLine($"Input: '{input}'");
        _output.WriteLine($"Tokens: [{string.Join(", ", tokens.Select(t => $"'{t}'"))}]");
        
        // .( should create a token, and WORD1 and WORD2 should also be tokens
        Assert.Equal(3, tokens.Count);
        Assert.Equal("WORD1", tokens[0]);
        Assert.Equal(".( test message )", tokens[1]);
        Assert.Equal("WORD2", tokens[2]);
    }

    [Fact]
    public void Tokenizer_DotParen_EmptyMessage_ShouldCreateToken()
    {
        var input = ".() WORD";
        var tokens = Forth.Core.Interpreter.Tokenizer.Tokenize(input);
        
        _output.WriteLine($"Input: '{input}'");
        _output.WriteLine($"Tokens: [{string.Join(", ", tokens.Select(t => $"'{t}'"))}]");
        
        // Empty .( should create a token
        Assert.Equal(2, tokens.Count);
        Assert.Equal(".()", tokens[0]);
        Assert.Equal("WORD", tokens[1]);
    }

    [Fact]
    public void Tokenizer_DotParen_WithSpecialChars_ShouldPreserveContent()
    {
        var input = ".( Test: 1+2=3 \"quoted\" )";
        var tokens = Forth.Core.Interpreter.Tokenizer.Tokenize(input);
        
        _output.WriteLine($"Input: '{input}'");
        _output.WriteLine($"Tokens: [{string.Join(", ", tokens.Select(t => $"'{t}'"))}]");
        
        // Special characters should be preserved in the token
        Assert.Single(tokens);
        Assert.Equal(".( Test: 1+2=3 \"quoted\" )", tokens[0]);
    }

    [Fact]
    public void Tokenizer_DotParen_MissingClosingParen_ShouldThrow()
    {
        var input = ".( unclosed message";
        
        _output.WriteLine($"Input: '{input}'");
        
        // Missing closing paren should throw
        var ex = Assert.Throws<Forth.Core.ForthException>(() => 
            Forth.Core.Interpreter.Tokenizer.Tokenize(input));
        
        Assert.Contains("missing closing )", ex.Message);
    }

    [Fact]
    public void Tokenizer_DotParen_Multiple_ShouldCreateMultipleTokens()
    {
        var input = ".( First ) .( Second )";
        var tokens = Forth.Core.Interpreter.Tokenizer.Tokenize(input);
        
        _output.WriteLine($"Input: '{input}'");
        _output.WriteLine($"Tokens: [{string.Join(", ", tokens.Select(t => $"'{t}'"))}]");
        
        // Both .( ) constructs should create tokens
        Assert.Equal(2, tokens.Count);
        Assert.Equal(".( First )", tokens[0]);
        Assert.Equal(".( Second )", tokens[1]);
    }

    [Fact]
    public void Tokenizer_DotParen_WithLeadingAndTrailingSpaces_ShouldPreserve()
    {
        var input = ".(   spaced   )";
        var tokens = Forth.Core.Interpreter.Tokenizer.Tokenize(input);
        
        _output.WriteLine($"Input: '{input}'");
        _output.WriteLine($"Tokens: [{string.Join(", ", tokens.Select(t => $"'{t}'"))}]");
        
        // Spaces should be preserved in the token
        Assert.Single(tokens);
        Assert.Equal(".(   spaced   )", tokens[0]);
    }

    [Fact]
    public void Tokenizer_DotParen_NotConfusedWithDot_ShouldNotCreateDotToken()
    {
        var input = ".( message )";
        var tokens = Forth.Core.Interpreter.Tokenizer.Tokenize(input);
        
        _output.WriteLine($"Input: '{input}'");
        _output.WriteLine($"Tokens: [{string.Join(", ", tokens.Select(t => $"'{t}'"))}]");
        
        // Should create a single .( ) token, NOT a "." token
        Assert.Single(tokens);
        Assert.DoesNotContain(".", tokens);
        Assert.Equal(".( message )", tokens[0]);
    }

    [Fact]
    public void Tokenizer_DotParen_NotConfusedWithParenComment()
    {
        // Regular paren comment should be stripped
        var input1 = "WORD1 ( comment ) WORD2";
        var tokens1 = Forth.Core.Interpreter.Tokenizer.Tokenize(input1);
        
        // .( should create a token
        var input2 = "WORD1 .( message ) WORD2";
        var tokens2 = Forth.Core.Interpreter.Tokenizer.Tokenize(input2);
        
        _output.WriteLine($"Comment input: '{input1}' -> tokens: {tokens1.Count}");
        _output.WriteLine($"DotParen input: '{input2}' -> tokens: {tokens2.Count}");
        
        // Regular comment produces 2 tokens (WORD1, WORD2)
        Assert.Equal(2, tokens1.Count);
        Assert.Equal("WORD1", tokens1[0]);
        Assert.Equal("WORD2", tokens1[1]);
        
        // .( produces 3 tokens (WORD1, .( message ), WORD2)
        Assert.Equal(3, tokens2.Count);
        Assert.Equal("WORD1", tokens2[0]);
        Assert.Equal(".( message )", tokens2[1]);
        Assert.Equal("WORD2", tokens2[2]);
    }

    #endregion

    #region Other Special Form Regression Tests

    [Fact]
    public void Tokenizer_SQuote_ShouldSkipOneLeadingSpace()
    {
        var input = "S\" hello\"";
        var tokens = Forth.Core.Interpreter.Tokenizer.Tokenize(input);
        
        _output.WriteLine($"Input: '{input}'");
        _output.WriteLine($"Tokens: [{string.Join(", ", tokens.Select(t => $"'{t}'"))}]");
        
        Assert.Equal(2, tokens.Count);
        Assert.Equal("S\"", tokens[0]);
        Assert.Equal("\"hello\"", tokens[1]);
    }

    [Fact]
    public void Tokenizer_DotQuote_ShouldHandleWhitespace()
    {
        var input = ".\"  test  \"";
        var tokens = Forth.Core.Interpreter.Tokenizer.Tokenize(input);
        
        _output.WriteLine($"Input: '{input}'");
        _output.WriteLine($"Tokens: [{string.Join(", ", tokens.Select(t => $"'{t}'"))}]");
        
        Assert.Equal(2, tokens.Count);
        Assert.Equal(".\"", tokens[0]);
        // Leading whitespace after ." is skipped
        Assert.Contains("test", tokens[1]);
    }

    [Fact]
    public void Tokenizer_BracketTickBracket_ShouldBeOneToken()
    {
        var input = "['] WORD";
        var tokens = Forth.Core.Interpreter.Tokenizer.Tokenize(input);
        
        _output.WriteLine($"Input: '{input}'");
        _output.WriteLine($"Tokens: [{string.Join(", ", tokens.Select(t => $"'{t}'"))}]");
        
        Assert.Equal(2, tokens.Count);
        Assert.Equal("[']", tokens[0]);
        Assert.Equal("WORD", tokens[1]);
    }

    [Fact]
    public void Tokenizer_LOCAL_ShouldBeOneToken()
    {
        var input = "(LOCAL) name";
        var tokens = Forth.Core.Interpreter.Tokenizer.Tokenize(input);
        
        _output.WriteLine($"Input: '{input}'");
        _output.WriteLine($"Tokens: [{string.Join(", ", tokens.Select(t => $"'{t}'"))}]");
        
        Assert.Equal(2, tokens.Count);
        Assert.Equal("(LOCAL)", tokens[0]);
        Assert.Equal("name", tokens[1]);
    }

    [Fact]
    public void Tokenizer_DotBracketS_ShouldBeOneToken()
    {
        var input = ".[S] test";
        var tokens = Forth.Core.Interpreter.Tokenizer.Tokenize(input);
        
        _output.WriteLine($"Input: '{input}'");
        _output.WriteLine($"Tokens: [{string.Join(", ", tokens.Select(t => $"'{t}'"))}]");
        
        Assert.Equal(2, tokens.Count);
        Assert.Equal(".[S]", tokens[0]);
        Assert.Equal("test", tokens[1]);
    }

    [Fact]
    public void Tokenizer_Semicolon_ShouldBeOwnToken()
    {
        var input = ": WORD ; test";
        var tokens = Forth.Core.Interpreter.Tokenizer.Tokenize(input);
        
        _output.WriteLine($"Input: '{input}'");
        _output.WriteLine($"Tokens: [{string.Join(", ", tokens.Select(t => $"'{t}'"))}]");
        
        Assert.Equal(4, tokens.Count);
        Assert.Equal(":", tokens[0]);
        Assert.Equal("WORD", tokens[1]);
        Assert.Equal(";", tokens[2]);
        Assert.Equal("test", tokens[3]);
    }

    #endregion
}
