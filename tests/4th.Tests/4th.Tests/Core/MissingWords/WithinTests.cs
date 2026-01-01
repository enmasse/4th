using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.MissingWords
{
    public class WithinTests
    {
        private static ForthInterpreter New() => new();

        [Fact]
        public async Task Within_True_When_InRange()
        {
            var f = New();
            Assert.True(await f.EvalAsync("5 0 10 WITHIN"));
            Assert.Single(f.Stack);
            Assert.Equal(-1L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Within_False_When_BelowLow()
        {
            var f = New();
            Assert.True(await f.EvalAsync("-1 0 10 WITHIN"));
            Assert.Single(f.Stack);
            Assert.Equal(0L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Within_False_When_AtHighBoundary_Exclusive()
        {
            var f = New();
            Assert.True(await f.EvalAsync("10 0 10 WITHIN"));
            Assert.Single(f.Stack);
            Assert.Equal(0L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Within_True_When_AtLowBoundary_Inclusive()
        {
            var f = New();
            Assert.True(await f.EvalAsync("0 0 10 WITHIN"));
            Assert.Single(f.Stack);
            Assert.Equal(-1L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Within_With_Negatives()
        {
            var f = New();
            Assert.True(await f.EvalAsync("-5 -10 0 WITHIN"));
            Assert.Single(f.Stack);
            Assert.Equal(-1L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Within_False_When_Outside_Negatives()
        {
            var f = New();
            Assert.True(await f.EvalAsync("-11 -10 0 WITHIN"));
            Assert.Single(f.Stack);
            Assert.Equal(0L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Within_Order_Matters()
        {
            var f = New();
            // x low high ; here low > high should produce false for any x per standard expectations
            Assert.True(await f.EvalAsync("5 10 0 WITHIN"));
            Assert.Single(f.Stack);
            Assert.Equal(0L, (long)f.Stack[0]);
        }
    }
}
