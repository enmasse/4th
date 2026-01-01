using System.Threading.Tasks;
using Xunit;
using Forth.Core.Interpreter;

namespace Forth.Tests.Core.ControlFlow
{
    /// <summary>
    /// Regression tests for bracketed conditionals ([IF] [ELSE] [THEN]) focusing on nesting,
    /// mixed separated/composite forms, and multi-line constructs.
    /// </summary>
    public class BracketConditionalsNestingRegressionTests
    {
        private static ForthInterpreter New() => new();

        [Fact]
        public async Task Nested_TwoLevels_OuterTrue_InnerTrue_ExecutesInnerThen()
        {
            var f = New();
            Assert.True(await f.EvalAsync("-1 [IF] -1 [IF] 123 [THEN] [ELSE] 999 [THEN]"));
            Assert.Single(f.Stack);
            Assert.Equal(123L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Nested_TwoLevels_OuterTrue_InnerFalse_TakesInnerElse()
        {
            var f = New();
            Assert.True(await f.EvalAsync("-1 [IF] 0 [IF] 111 [ELSE] 222 [THEN] [ELSE] 333 [THEN]"));
            Assert.Single(f.Stack);
            Assert.Equal(222L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Nested_TwoLevels_OuterFalse_SkipsInnerEntirely()
        {
            var f = New();
            Assert.True(await f.EvalAsync("0 [IF] -1 [IF] 111 [THEN] [ELSE] 222 [THEN]"));
            Assert.Single(f.Stack);
            Assert.Equal(222L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Nested_ThreeLevels_MixedForms_AllTrue_ExecutesDeepThen()
        {
            var f = New();
            // Mixed separated and composite bracket forms
            Assert.True(await f.EvalAsync("-1 [ IF ] -1 [IF] -1 [ IF ] 777 [ THEN ] [THEN] [ THEN ]"));
            Assert.Single(f.Stack);
            Assert.Equal(777L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Nested_ThreeLevels_InnerElse_Selected()
        {
            var f = New();
            Assert.True(await f.EvalAsync("-1 [IF] -1 [IF] 0 [IF] 1 [ELSE] 2 [THEN] [ELSE] 3 [THEN]"));
            Assert.Single(f.Stack);
            Assert.Equal(2L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task MultiLine_Nesting_OuterTrue_InnerFalse_TakesElse()
        {
            var f = New();
            var src = string.Join('\n', new[]
            {
                "-1 [IF]",
                " 0 [IF] 10 [ELSE] 20 [THEN]",
                "[THEN]"
            });
            Assert.True(await f.EvalAsync(src));
            Assert.Single(f.Stack);
            Assert.Equal(20L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task MultiLine_Nesting_OuterFalse_SkipsAllUntilThen()
        {
            var f = New();
            var src = string.Join('\n', new[]
            {
                "0 [IF]",
                "  -1 [IF] 999 [THEN]",
                "[THEN]",
                "42" // After closing THEN outer, push 42
            });
            Assert.True(await f.EvalAsync(src));
            Assert.Single(f.Stack);
            Assert.Equal(42L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task MultiLine_MixedSeparatedForms_WithElse()
        {
            var f = New();
            var src = string.Join('\n', new[]
            {
                "-1 [ IF ]",
                "  0 [IF] 1 [ ELSE ] 2 [THEN]",
                "[ THEN ]"
            });
            Assert.True(await f.EvalAsync(src));
            Assert.Single(f.Stack);
            Assert.Equal(2L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task DeepNesting_EmptyBranches_AreHandled()
        {
            var f = New();
            Assert.True(await f.EvalAsync("-1 [IF] -1 [IF] [THEN] [ELSE] [THEN]"));
            Assert.Empty(f.Stack);
        }

        [Fact]
        public async Task MultipleAdjacentConditionals_WithNesting()
        {
            var f = New();
            Assert.True(await f.EvalAsync("-1 [IF] 1 [THEN] -1 [IF] -1 [IF] 2 [THEN] [THEN] -1 [IF] 3 [THEN]"));
            Assert.Equal(3, f.Stack.Count);
            Assert.Equal(1L, (long)f.Stack[0]);
            Assert.Equal(2L, (long)f.Stack[1]);
            Assert.Equal(3L, (long)f.Stack[2]);
        }
    }
}
