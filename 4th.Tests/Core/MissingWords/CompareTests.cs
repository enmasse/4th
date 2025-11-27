using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.MissingWords
{
    public class CompareTests
    {
        private static ForthInterpreter New() => new();

        [Fact]
        public async Task Compare_Equal_Strings()
        {
            var f = New();
            Assert.True(await f.EvalAsync("S\" hello\" S\" hello\" COMPARE"));
            Assert.Single(f.Stack);
            Assert.Equal(0L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Compare_First_Less()
        {
            var f = New();
            Assert.True(await f.EvalAsync("S\" apple\" S\" banana\" COMPARE"));
            Assert.Single(f.Stack);
            Assert.Equal(-1L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Compare_First_Greater()
        {
            var f = New();
            Assert.True(await f.EvalAsync("S\" zebra\" S\" apple\" COMPARE"));
            Assert.Single(f.Stack);
            Assert.Equal(1L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Compare_Empty_Strings()
        {
            var f = New();
            Assert.True(await f.EvalAsync("S\" \" S\" \" COMPARE"));
            Assert.Single(f.Stack);
            Assert.Equal(0L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Compare_Different_Lengths()
        {
            var f = New();
            Assert.True(await f.EvalAsync("S\" a\" S\" ab\" COMPARE"));
            Assert.Single(f.Stack);
            Assert.Equal(-1L, (long)f.Stack[0]);
        }
    }
}