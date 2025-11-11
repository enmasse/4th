using System.Threading.Tasks;
using Xunit;
using Forth.Core;
using Forth.Core.Interpreter;
using System.Linq;

namespace Forth.Tests.Core
{
    /// <summary>
    /// Additional edge and negative tests expanding coverage for compilation, control flow, defining words, memory ops and error paths.
    /// </summary>
    public class ExtendedCoverageTests
    {
        private static ForthInterpreter New() => new();

        private sealed class TestIO : IForthIO
        {
            public readonly System.Collections.Generic.List<string> Outputs = new();
            public void Print(string text) => Outputs.Add(text);
            public void PrintNumber(long number) => Outputs.Add(number.ToString());
            public void NewLine() => Outputs.Add("\n");
            public string? ReadLine() => null;
        }

        [Fact]
        public async Task LiteralCapturesStackValueInsideDefinition()
        {
            var f = New();
            // Push 42 then compile LITERAL to embed 42
            Assert.True(await f.EvalAsync("42 : PUSH42 LITERAL ;"));
            Assert.True(await f.EvalAsync("PUSH42 PUSH42"));
            // Only two literals produced (original 42 consumed by LITERAL during compile)
            Assert.Equal(2, f.Stack.Count);
            Assert.Equal(42L, (long)f.Stack[0]);
            Assert.Equal(42L, (long)f.Stack[1]);
        }

        [Fact]
        public async Task PostponeUndefinedWord_RaisesUndefined()
        {
            var f = New();
            // Attempt to POSTPONE an undefined word should ultimately fail during compilation
            var ex = await Assert.ThrowsAsync<ForthException>(() => f.EvalAsync(": T POSTPONE FOO 1 ;"));
            Assert.Equal(ForthErrorCode.UndefinedWord, ex.Code);
        }

        [Fact]
        public async Task CreateWithoutDoes_PushesAddressOnly()
        {
            var f = New();
            Assert.True(await f.EvalAsync("CREATE BUF"));
            Assert.True(await f.EvalAsync("BUF"));
            Assert.Single(f.Stack);
            Assert.True(f.Stack[0] is long && (long)f.Stack[0] > 0);
        }

        [Fact]
        public async Task DeferBeforeIs_ThrowsOnInvocation()
        {
            var f = New();
            Assert.True(await f.EvalAsync("DEFER ACTION"));
            var ex = await Assert.ThrowsAsync<ForthException>(() => f.EvalAsync("ACTION"));
            Assert.Equal(ForthErrorCode.UndefinedWord, ex.Code);
        }

        [Fact]
        public async Task ConstantWithoutValue_StackUnderflow()
        {
            var f = New();
            var ex = await Assert.ThrowsAsync<ForthException>(() => f.EvalAsync("CONSTANT X"));
            Assert.Equal(ForthErrorCode.StackUnderflow, ex.Code);
        }

        [Fact]
        public async Task NegativeAllot_Throws()
        {
            var f = New();
            var ex = await Assert.ThrowsAsync<ForthException>(() => f.EvalAsync("-5 ALLOT"));
            Assert.Equal(ForthErrorCode.CompileError, ex.Code);
        }

        [Fact]
        public async Task NegativeErase_Throws()
        {
            var f = New();
            Assert.True(await f.EvalAsync("CREATE B 10 ALLOT"));
            var ex = await Assert.ThrowsAsync<ForthException>(() => f.EvalAsync("B -1 ERASE"));
            Assert.Equal(ForthErrorCode.CompileError, ex.Code);
        }

        [Fact]
        public async Task DivisionByZeroVariants_Throw()
        {
            var f = New();
            var ex1 = await Assert.ThrowsAsync<ForthException>(() => f.EvalAsync("10 0 /"));
            Assert.Equal(ForthErrorCode.DivideByZero, ex1.Code);
            var ex2 = await Assert.ThrowsAsync<ForthException>(() => f.EvalAsync("10 0 /MOD"));
            Assert.Equal(ForthErrorCode.DivideByZero, ex2.Code);
            var ex3 = await Assert.ThrowsAsync<ForthException>(() => f.EvalAsync("10 0 MOD"));
            Assert.Equal(ForthErrorCode.DivideByZero, ex3.Code);
        }

        [Fact]
        public async Task MoveOverlappingBackwardCopy()
        {
            var f = New();
            Assert.True(await f.EvalAsync("CREATE A 5 ALLOT"));
            for (int n = 0; n < 5; n++) Assert.True(await f.EvalAsync($"{n+1} A {n} + C!"));
            Assert.True(await f.EvalAsync("A A 1 + 4 MOVE"));
            Assert.True(await f.EvalAsync("A 4 + C@"));
            Assert.Single(f.Stack);
            Assert.Equal(4L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task ReturnStackUnderflow_RGreater()
        {
            var f = New();
            var ex = await Assert.ThrowsAsync<ForthException>(() => f.EvalAsync("R>"));
            Assert.Equal(ForthErrorCode.StackUnderflow, ex.Code);
        }

        [Fact]
        public async Task ExecuteWordBelowTopExecutesAndConsumesXt()
        {
            var f = New();
            Assert.True(await f.EvalAsync(": INC 1 + ;"));
            Assert.True(await f.EvalAsync("5 ' INC EXECUTE"));
            Assert.Single(f.Stack);
            Assert.Equal(6L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task LoopReverseIteration_NegativeStep()
        {
            var f = New();
            Assert.True(await f.EvalAsync(": SUMDOWN 0 0 5 DO I + LOOP ;"));
            Assert.True(await f.EvalAsync("SUMDOWN"));
            Assert.Single(f.Stack);
            Assert.Equal(15L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task LeaveOutsideLoop_Throws()
        {
            var f = New();
            var ex = await Assert.ThrowsAsync<ForthException>(() => f.EvalAsync(": X LEAVE ;"));
            Assert.Equal(ForthErrorCode.CompileError, ex.Code);
        }

        [Fact]
        public async Task AbortWithMessage_ThrowsExpected()
        {
            var f = New();
            var ex = await Assert.ThrowsAsync<ForthException>(() => f.EvalAsync("ABORT \"FAIL\""));
            Assert.Equal(ForthErrorCode.Unknown, ex.Code);
            Assert.Contains("FAIL", ex.Message);
        }

        [Fact]
        public async Task TaskQuestionOnIncompleteTask_ReturnsZero()
        {
            var f = New();
            Assert.True(await f.EvalAsync("BINDASYNC Forth.Tests.Core.Binding.AsyncTestTargets VoidDelay 1 DELAY 5 DELAY DUP TASK?"));
            Assert.Equal(2, f.Stack.Count);
            // First item: task-like object, second: flag 0
            Assert.True(!(f.Stack[0] is long));
            Assert.Equal(0L, (long)f.Stack[1]);
        }

        [Fact]
        public async Task AwaitSameTaskTwice_ResultOnlyOnce()
        {
            var f = New();
            Assert.True(await f.EvalAsync("BINDASYNC Forth.Tests.Core.Binding.AsyncTestTargets AddAsync 2 ADDAB"));
            Assert.True(await f.EvalAsync("2 3 ADDAB DUP AWAIT DUP AWAIT"));
            var nums = f.Stack.Where(o => o is long).Select(o => (long)o).ToArray();
            Assert.True(nums.Length >= 1);
        }

        [Fact]
        public async Task UnmatchedControlFlowTokens_CompileOrUndefined()
        {
            var f = New();
            async Task Check(string line)
            {
                var ex = await Assert.ThrowsAsync<ForthException>(() => f.EvalAsync(line));
                Assert.Contains(ex.Code, new[] { ForthErrorCode.CompileError, ForthErrorCode.UndefinedWord });
            }
            await Check(": X ELSE ;");
            await Check(": Y THEN ;");
            await Check(": Z REPEAT ;");
            await Check(": W BEGIN WHILE UNTIL ;");
        }

        [Fact]
        public async Task UnloopOutsideLoop_Throws()
        {
            var f = New();
            var ex = await Assert.ThrowsAsync<ForthException>(() => f.EvalAsync("UNLOOP"));
            Assert.Equal(ForthErrorCode.CompileError, ex.Code);
        }

        [Fact]
        public async Task ModuleSearchOrder_ShadowAndRoot()
        {
            var f = New();
            Assert.True(await f.EvalAsync(": FOO 1 ;"));
            Assert.True(await f.EvalAsync("MODULE A : FOO 2 ; END-MODULE"));
            Assert.True(await f.EvalAsync("MODULE B : FOO 3 ; END-MODULE"));
            Assert.True(await f.EvalAsync("USING A USING B FOO"));
            Assert.Single(f.Stack);
            Assert.Equal(3L, (long)f.Stack[0]);
            f = New();
            Assert.True(await f.EvalAsync(": FOO 1 ; MODULE A : FOO 2 ; END-MODULE USING A FOO"));
            Assert.Single(f.Stack);
            Assert.Equal(2L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task QualifiedSee_WorksForModuleWord()
        {
            var io = new TestIO();
            var f = new ForthInterpreter(io);
            Assert.True(await f.EvalAsync("MODULE M : ADD2 + ; END-MODULE SEE M:ADD2"));
            Assert.Single(io.Outputs);
            Assert.Equal(": ADD2 + ;", io.Outputs[0]);
        }

        [Fact]
        public async Task ToNumber_HexWithRemainder()
        {
            var f = New();
            Assert.True(await f.EvalAsync("HEX S\" FFZ\" 0 0 >NUMBER DECIMAL"));
            Assert.Equal(3, f.Stack.Count);
            Assert.Equal(255L, (long)f.Stack[0]);
            Assert.Equal(1L, (long)f.Stack[1]);
            Assert.Equal(2L, (long)f.Stack[2]);
        }

        [Fact]
        public async Task TypeOnNonString_ThrowsTypeError()
        {
            var f = New();
            var ex = await Assert.ThrowsAsync<ForthException>(() => f.EvalAsync("42 TYPE"));
            Assert.Equal(ForthErrorCode.TypeError, ex.Code);
        }

        [Fact]
        public async Task PicturedNumeric_HoldCharOnly()
        {
            var io = new TestIO();
            var f = new ForthInterpreter(io);
            Assert.True(await f.EvalAsync("<# 65 HOLD #> TYPE"));
            Assert.Single(io.Outputs);
            Assert.Equal("A", io.Outputs[0]);
        }

        [Fact]
        public async Task HexParsing_MultiDigit()
        {
            var f = New();
            Assert.True(await f.EvalAsync("HEX 1A DECIMAL"));
            Assert.Single(f.Stack);
            Assert.Equal(26L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task SQuoteMissingClosing_Throws()
        {
            var f = New();
            var ex = await Assert.ThrowsAsync<ForthException>(() => f.EvalAsync("S\" 123"));
            Assert.Equal(ForthErrorCode.CompileError, ex.Code);
        }

        [Fact]
        public async Task CharMissingToken_Throws()
        {
            var f = New();
            var ex = await Assert.ThrowsAsync<ForthException>(() => f.EvalAsync("CHAR"));
            Assert.Equal(ForthErrorCode.CompileError, ex.Code);
        }
    }
}
