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
}
