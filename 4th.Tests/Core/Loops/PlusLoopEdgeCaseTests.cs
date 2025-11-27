using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FourTh.Tests.Core.Loops
{
    public class PlusLoopEdgeCaseTests
    {
        private static Forth.Core.Interpreter.ForthInterpreter NewIntr() => new();

        [Fact]
        public async Task PlusLoop_CrossesLimit_TerminatesAfterBody()
        {
            var i = NewIntr();
            await i.EvalAsync(": PUSHI I ;");
            // Start at 0, limit 5, step 3 -> indices: 0,3 then next 6 crosses -> terminate
            var ok = await i.EvalAsync(": T 5 0 DO PUSHI 3 +LOOP ; T");
            Assert.True(ok);
            var stack = i.Stack.Select(Forth.Core.Interpreter.ForthInterpreter.ToLong).ToArray();
            Assert.Equal(new long[] { 0, 3 }, stack);
        }

        [Fact]
        public async Task PlusLoop_CrossesLimitDescending_TerminatesAfterBody()
        {
            var i = NewIntr();
            await i.EvalAsync(": PUSHI I ;");
            // Start at 0, limit -5, step -3 -> indices: 0,-3 then next -6 crosses -> terminate
            var ok = await i.EvalAsync(": T -5 0 DO PUSHI -3 +LOOP ; T");
            Assert.True(ok);
            var stack = i.Stack.Select(Forth.Core.Interpreter.ForthInterpreter.ToLong).ToArray();
            Assert.Equal(new long[] { 0, -3 }, stack);
        }

        [Fact]
        public async Task PlusLoop_ZeroStep_DefaultsToDirection()
        {
            var i = NewIntr();
            await i.EvalAsync(": PUSHI I ;");
            // Zero step should be treated as 1 for ascending to avoid infinite loop
            var ok = await i.EvalAsync(": T 5 0 DO PUSHI 0 +LOOP ; T");
            Assert.True(ok);
            var stack = i.Stack.Select(Forth.Core.Interpreter.ForthInterpreter.ToLong).ToArray();
            Assert.Equal(new long[] { 0, 1, 2, 3, 4 }, stack);
        }

        [Fact]
        public async Task QDo_DescendingRange_RunsWhenStartNotEqualLimit()
        {
            var i = NewIntr();
            await i.EvalAsync(": PUSHI I ;");
            // ?DO executes when start != limit; ensure descending range works
            var ok = await i.EvalAsync(": T -3 0 ?DO PUSHI -1 +LOOP ; T");
            Assert.True(ok);
            var stack = i.Stack.Select(Forth.Core.Interpreter.ForthInterpreter.ToLong).ToArray();
            Assert.Equal(new long[] { 0, -1, -2 }, stack);
        }
    }
}
