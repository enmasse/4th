using System.Threading.Tasks;
using Xunit;
using Forth.Core.Interpreter;

namespace Forth.Tests.Core.ControlFlow
{
    public class BracketConditionalsTests
    {
        // Intention: Verify that bracketed IF executes the then-part when condition is non-zero
        [Fact]
        public async Task BracketIF_True_ExecutesFirstPart()
        {
            var forth = new ForthInterpreter();
            Assert.True(await forth.EvalAsync("1 [IF] 2 [ELSE] 3 [THEN]"));
            Assert.Single(forth.Stack);
            Assert.Equal(2L, (long)forth.Stack[0]);
        }

        // Intention: Verify that bracketed IF executes the else-part when condition is zero
        [Fact]
        public async Task BracketIF_False_ExecutesElsePart()
        {
            var forth = new ForthInterpreter();
            Assert.True(await forth.EvalAsync("0 [IF] 2 [ELSE] 5 [THEN]"));
            Assert.Single(forth.Stack);
            Assert.Equal(5L, (long)forth.Stack[0]);
        }

        // Intention: Verify nested bracketed IF/ELSE/THEN blocks skip correctly and choose the right branch
        [Fact]
        public async Task BracketIF_Nested_SkipsCorrectly()
        {
            var forth = new ForthInterpreter();
            // Outer true, inner false chooses inner ELSE
            Assert.True(await forth.EvalAsync("1 [IF] 0 [IF] 10 [ELSE] 20 [THEN] [ELSE] 30 [THEN]"));
            Assert.Single(forth.Stack);
            Assert.Equal(20L, (long)forth.Stack[0]);
        }

        // Intention: Verify interpretive IF (without brackets) runs the then-part when condition is non-zero
        [Fact]
        public async Task InterpretIF_True_RunsThenPart()
        {
            var forth = new ForthInterpreter();
            Assert.True(await forth.EvalAsync(": TIF1 1 IF 11 ELSE 22 THEN ;"));
            Assert.True(await forth.EvalAsync("TIF1"));
            Assert.Single(forth.Stack);
            Assert.Equal(11L, (long)forth.Stack[0]);
        }

        // Intention: Verify interpretive IF (without brackets) runs the else-part when condition is zero
        [Fact]
        public async Task InterpretIF_False_RunsElsePart()
        {
            var forth = new ForthInterpreter();
            Assert.True(await forth.EvalAsync(": TIF2 0 IF 11 ELSE 22 THEN ;"));
            Assert.True(await forth.EvalAsync("TIF2"));
            Assert.Single(forth.Stack);
            Assert.Equal(22L, (long)forth.Stack[0]);
        }
    }
}
