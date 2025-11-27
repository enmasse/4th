using System.Threading.Tasks;
using Xunit;

namespace FourTh.Tests.Core.Loops
{
    public class ConditionalDoAndPlusLoopTests
    {
        private static Forth.Core.Interpreter.ForthInterpreter NewIntr() => new();

        [Fact]
        public async Task QDo_SkipsBody_WhenStartEqualsLimit()
        {
            var i = NewIntr();
            // Define a word that would push 1 inside the loop
            await i.EvalAsync(": PUSH1 1 ;");
            // Compile loop in a definition; ?DO should skip body entirely when start==limit
            var ok = await i.EvalAsync(": TEST1 0 0 ?DO PUSH1 LOOP ; TEST1");
            Assert.True(ok);
            Assert.Empty(i.Stack);
        }

        [Fact]
        public async Task QDo_RunsBody_WhenStartNotEqualLimit()
        {
            var i = NewIntr();
            await i.EvalAsync(": PUSH1 1 ;");
            var ok = await i.EvalAsync(": TEST2 3 0 ?DO PUSH1 LOOP ; TEST2");
            Assert.True(ok);
            // Loop should run for indices 0,1,2 -> push three 1s
            var stack = i.Stack;
            Assert.Equal(3, stack.Count);
            Assert.All(stack, v => Assert.Equal(1L, Forth.Core.Interpreter.ForthInterpreter.ToLong(v)));
        }

        [Fact]
        public async Task PlusLoop_PositiveStep_TerminatesAtLimit()
        {
            var i = NewIntr();
            await i.EvalAsync(": PUSHI I ;");
            // Collect loop indices using I with +LOOP step 2 from 0 to 7 (exclusive)
            var ok = await i.EvalAsync(": TEST3 7 0 DO PUSHI 2 +LOOP ; TEST3");
            Assert.True(ok);
            var stack = i.Stack;
            // Indices: 0,2,4,6
            Assert.Equal(new long[] { 0, 2, 4, 6 }, stack.Select(Forth.Core.Interpreter.ForthInterpreter.ToLong));
        }

        [Fact]
        public async Task PlusLoop_NegativeStep_TerminatesAtLimit()
        {
            var i = NewIntr();
            await i.EvalAsync(": PUSHI I ;");
            // From 0 down to -7 with step -3 -> 0,-3,-6
            var ok = await i.EvalAsync(": TEST4 -7 0 DO PUSHI -3 +LOOP ; TEST4");
            Assert.True(ok);
            var stack = i.Stack;
            Assert.Equal(new long[] { 0, -3, -6 }, stack.Select(Forth.Core.Interpreter.ForthInterpreter.ToLong));
        }

        [Fact]
        public async Task PlusLoop_Leave_ExitsEarly()
        {
            var i = NewIntr();
            // Leave when index reaches 3; capture raw index before stepping
            var ok = await i.EvalAsync(": TEST5 10 0 DO I 3 = IF LEAVE THEN I 1 +LOOP ; TEST5");
            Assert.True(ok);
            var stack = i.Stack;
            // Captured indices before LEAVE: 0,1,2
            Assert.Equal(new long[] { 0, 1, 2 }, stack.Select(Forth.Core.Interpreter.ForthInterpreter.ToLong));
        }
    }
}
