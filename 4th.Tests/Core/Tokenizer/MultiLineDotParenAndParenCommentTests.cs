using System.Linq;
using Xunit;

namespace Forth.Tests.Core.Tokenizer
{
    public class MultiLineDotParenAndParenCommentTests
    {
        [Fact]
        public void Tokenizer_DotParen_ShouldSpanMultipleLines()
        {
            var input = ".( Hello\nWorld )";
            var tokens = Forth.Core.Interpreter.Tokenizer.Tokenize(input);
            Assert.Single(tokens);
            Assert.Equal(".( Hello\nWorld )", tokens[0]);
        }

        [Fact]
        public void Tokenizer_DotParen_EmptyAcrossLines_ShouldCreateToken()
        {
            var input = ".(\n) NEXT";
            var tokens = Forth.Core.Interpreter.Tokenizer.Tokenize(input);
            Assert.Equal(2, tokens.Count);
            Assert.Equal(".(\n)", tokens[0]);
            Assert.Equal("NEXT", tokens[1]);
        }

        [Fact]
        public void Tokenizer_ParenComment_ShouldSpanMultipleLines()
        {
            var input = "WORD1 ( comment\nmore text ) WORD2";
            var tokens = Forth.Core.Interpreter.Tokenizer.Tokenize(input);
            Assert.Equal(2, tokens.Count);
            Assert.Equal("WORD1", tokens[0]);
            Assert.Equal("WORD2", tokens[1]);
        }

        [Fact]
        public void Tokenizer_ParenComment_MultiLine_ShouldConsumeUntilClosing()
        {
            var input = "WORD1 ( comment without close\nmore on next line ) AFTER";
            var tokens = Forth.Core.Interpreter.Tokenizer.Tokenize(input);
            // Paren comment should consume until the closing ')', across newline
            Assert.Equal(2, tokens.Count);
            Assert.Equal("WORD1", tokens[0]);
            Assert.Equal("AFTER", tokens[1]);
            // No stray ')' token
            Assert.DoesNotContain(")", tokens);
        }
    }
}
