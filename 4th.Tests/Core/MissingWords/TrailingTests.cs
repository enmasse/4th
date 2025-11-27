using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.MissingWords
{
    public class TrailingTests
    {
        private static ForthInterpreter New() => new();

        [Fact]
        public async Task Trailing_NoTrailingSpaces()
        {
            var f = New();
            Assert.True(await f.EvalAsync("S\" hello\" -TRAILING"));
            Assert.Equal(2, f.Stack.Count);
            Assert.IsType<long>(f.Stack[0]); // address
            Assert.Equal(5L, (long)f.Stack[1]);
        }

        [Fact]
        public async Task Trailing_WithTrailingSpaces()
        {
            var f = New();
            Assert.True(await f.EvalAsync("S\" hello \" -TRAILING"));
            Assert.Equal(2, f.Stack.Count);
            Assert.IsType<long>(f.Stack[0]); // address
            Assert.Equal(5L, (long)f.Stack[1]);
        }

        [Fact]
        public async Task Trailing_AllSpaces()
        {
            var f = New();
            Assert.True(await f.EvalAsync("S\"    \" -TRAILING"));
            Assert.Equal(2, f.Stack.Count);
            Assert.IsType<long>(f.Stack[0]); // address
            Assert.Equal(0L, (long)f.Stack[1]);
        }

        [Fact]
        public async Task Trailing_EmptyString()
        {
            var f = New();
            Assert.True(await f.EvalAsync("S\" \" -TRAILING"));
            Assert.Equal(2, f.Stack.Count);
            Assert.IsType<long>(f.Stack[0]); // address
            Assert.Equal(0L, (long)f.Stack[1]);
        }

        [Fact]
        public async Task Trailing_MultipleTrailingSpaces()
        {
            var f = New();
            Assert.True(await f.EvalAsync("S\" test   \" -TRAILING"));
            Assert.Equal(2, f.Stack.Count);
            Assert.IsType<long>(f.Stack[0]); // address
            Assert.Equal(4L, (long)f.Stack[1]);
        }
    }
}