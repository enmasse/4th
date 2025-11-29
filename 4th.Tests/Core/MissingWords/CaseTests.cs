using Forth.Core.Interpreter;
using Xunit;

namespace Forth.Tests.Core.MissingWords
{
    public class CaseTests
    {
        private static ForthInterpreter New() => new();

        [Fact]
        public async Task Case_Matches_First()
        {
            var f = New();
            Assert.True(await f.EvalAsync(": TEST CASE OF 5 100 ENDOF OF 10 200 ENDOF 300 ENDCASE ; 5 TEST"));
            Assert.Single(f.Stack);
            Assert.Equal(100L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Case_Matches_Second()
        {
            var f = New();
            Assert.True(await f.EvalAsync(": TEST CASE OF 5 100 ENDOF OF 10 200 ENDOF 300 ENDCASE ; 10 TEST"));
            Assert.Single(f.Stack);
            Assert.Equal(200L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Case_No_Match_Default()
        {
            var f = New();
            Assert.True(await f.EvalAsync(": TEST CASE OF 5 100 ENDOF OF 10 200 ENDOF 300 ENDCASE ; 15 TEST"));
            Assert.Single(f.Stack);
            Assert.Equal(300L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Case_Single_Branch()
        {
            var f = New();
            Assert.True(await f.EvalAsync(": TEST CASE OF 5 100 ENDOF 200 ENDCASE ; 5 TEST"));
            Assert.Single(f.Stack);
            Assert.Equal(100L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Case_No_Branches_Default()
        {
            var f = New();
            Assert.True(await f.EvalAsync(": TEST CASE 200 ENDCASE ; 5 TEST"));
            Assert.Single(f.Stack);
            Assert.Equal(200L, (long)f.Stack[0]);
        }
    }
}