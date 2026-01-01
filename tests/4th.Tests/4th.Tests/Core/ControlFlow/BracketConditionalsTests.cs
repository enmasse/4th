using System.Threading.Tasks;
using Xunit;
using Forth.Core.Interpreter;
using Forth.Core;

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

        // Intention: Verify empty then branch executes nothing
        [Fact]
        public async Task BracketIF_EmptyThenBranch()
        {
            var forth = new ForthInterpreter();
            Assert.True(await forth.EvalAsync("1 [IF] [THEN]"));
            Assert.Empty(forth.Stack);
        }

        // Intention: Verify empty else branch skips correctly
        [Fact]
        public async Task BracketIF_EmptyElseBranch()
        {
            var forth = new ForthInterpreter();
            Assert.True(await forth.EvalAsync("0 [IF] 42 [ELSE] [THEN]"));
            Assert.Empty(forth.Stack);
        }

        // Intention: Verify separated bracket forms work
        [Fact]
        public async Task BracketIF_SeparatedForms()
        {
            var forth = new ForthInterpreter();
            Assert.True(await forth.EvalAsync("1 [ IF ] 2 [ ELSE ] 3 [ THEN ]"));
            Assert.Single(forth.Stack);
            Assert.Equal(2L, (long)forth.Stack[0]);
        }

        // Intention: Verify mixed composite and separated forms
        [Fact]
        public async Task BracketIF_MixedForms()
        {
            var forth = new ForthInterpreter();
            Assert.True(await forth.EvalAsync("1 [IF] 2 [ ELSE ] 3 [THEN]"));
            Assert.Single(forth.Stack);
            Assert.Equal(2L, (long)forth.Stack[0]);
        }

        // Intention: Verify deep nesting
        [Fact]
        public async Task BracketIF_DeepNesting()
        {
            var forth = new ForthInterpreter();
            Assert.True(await forth.EvalAsync("1 [IF] 1 [IF] 1 [IF] 100 [THEN] [THEN] [THEN]"));
            Assert.Single(forth.Stack);
            Assert.Equal(100L, (long)forth.Stack[0]);
        }

        // Intention: Verify that false condition skips execution of words
        [Fact]
        public async Task BracketIF_SkipsExecutionWhenFalse()
        {
            var forth = new ForthInterpreter();
            Assert.True(await forth.EvalAsync("0 [IF] 1 2 3 [THEN]"));
            Assert.Empty(forth.Stack);
        }

        // Intention: Verify unmatched bracket with multi-line support
        // With ANS Forth multi-line support, an unmatched [IF] will skip subsequent lines
        // until [THEN] is found. Test that [ELSE] or [THEN] without [IF] throws error.
        [Fact]
        public async Task BracketIF_Unmatched_Throws()
        {
            var forth = new ForthInterpreter();
            // [ELSE] without [IF] should throw
            await Assert.ThrowsAsync<Forth.Core.ForthException>(() => forth.EvalAsync("[ELSE]"));
            
            // Create new interpreter for second test
            var forth2 = new ForthInterpreter();
            // [THEN] without [IF] should throw
            await Assert.ThrowsAsync<Forth.Core.ForthException>(() => forth2.EvalAsync("[THEN]"));
        }

        // Intention: Verify nested with empty branches
        [Fact]
        public async Task BracketIF_NestedEmptyBranches()
        {
            var forth = new ForthInterpreter();
            // True outer, false inner with empty else
            Assert.True(await forth.EvalAsync("1 [IF] 0 [IF] 10 [ELSE] [THEN] [THEN]"));
            Assert.Empty(forth.Stack);
        }

        // Intention: Verify interpretive IF (without brackets) runs the then-part when condition is non-zero
        [Fact]
        public async Task InterpretIF_True_RunsThenPart()
        {
            var forth = new ForthInterpreter();
            await forth.EvalAsync(": TIF1 1 IF 11 ELSE 22 THEN ;");
            Assert.True(await forth.EvalAsync("TIF1"));
            Assert.Single(forth.Stack);
            Assert.Equal(11L, (long)forth.Stack[0]);
        }

        // Intention: Verify interpretive IF (without brackets) runs the else-part when condition is zero
        [Fact]
        public async Task InterpretIF_False_RunsElsePart()
        {
            var forth = new ForthInterpreter();
            await forth.EvalAsync(": TIF2 0 IF 11 ELSE 22 THEN ;");
            Assert.True(await forth.EvalAsync("TIF2"));
            Assert.Single(forth.Stack);
            Assert.Equal(22L, (long)forth.Stack[0]);
        }
    }
}
