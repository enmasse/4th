using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.MissingWords
{
    public class AheadTests
    {
        private static ForthInterpreter New() => new();

        [Fact]
        public async Task Ahead_Skips_Code()
        {
            var f = New();
            Assert.True(await f.EvalAsync(": TEST AHEAD 1 THEN 2 ; TEST"));
            Assert.Single(f.Stack);
            Assert.Equal(2L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Ahead_In_If()
        {
            var f = New();
            Assert.True(await f.EvalAsync(": TEST 1 IF AHEAD 999 THEN 2 THEN 3 ; TEST"));
            Assert.Equal(2, f.Stack.Count);
            Assert.Equal(2L, (long)f.Stack[0]);
            Assert.Equal(3L, (long)f.Stack[1]);
        }

        [Fact]
        public async Task Ahead_Compiles_Correctly()
        {
            var f = New();
            Assert.True(await f.EvalAsync(": TEST AHEAD 1 2 + THEN 3 ; TEST"));
            Assert.Single(f.Stack);
            Assert.Equal(3L, (long)f.Stack[0]);
        }
    }
}