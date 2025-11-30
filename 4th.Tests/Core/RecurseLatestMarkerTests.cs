using System.Threading.Tasks;
using Xunit;
using Forth.Core.Interpreter;

namespace Forth.Tests.Core
{
    public class RecurseLatestMarkerTests
    {
        [Fact]
        public async Task Recurse_AllowsRecursiveDefinition()
        {
            var f = new ForthInterpreter();
            // SUMDOWN: n -- sum(1..n)
            Assert.True(await f.EvalAsync(": SUMDOWN DUP 0 = IF DROP 0 ELSE DUP 1- RECURSE + THEN ;"));
            Assert.True(await f.EvalAsync("5 SUMDOWN"));
            Assert.Single(f.Stack);
            Assert.Equal(15L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Latest_ReturnsLastDefinedXt()
        {
            var f = new ForthInterpreter();
            Assert.True(await f.EvalAsync(": A 42 ;"));
            // LATEST should push xt for A; EXECUTE leaves 42
            Assert.True(await f.EvalAsync("LATEST EXECUTE"));
            Assert.Single(f.Stack);
            Assert.Equal(42L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Marker_RestoresDefinitionsAndValues()
        {
            var f = new ForthInterpreter();
            // Create marker M, then define a word and a VALUE, then restore; both should be gone/reverted
            Assert.True(await f.EvalAsync("MARKER M"));
            // ANS Forth: VALUE requires an initial value from stack
            Assert.True(await f.EvalAsync(": T 1 ; 0 VALUE X 10 TO X"));
            // Sanity: both exist
            Assert.True(await f.EvalAsync("T X"));
            Assert.Equal(2, f.Stack.Count);
            Assert.Equal(1L, (long)f.Stack[0]);
            Assert.Equal(10L, (long)f.Stack[1]);
            // Execute marker to restore snapshot
            Assert.True(await f.EvalAsync("M"));
            // T and X should be undefined now
            await Assert.ThrowsAnyAsync<Forth.Core.ForthException>(() => f.EvalAsync("T"));
            await Assert.ThrowsAnyAsync<Forth.Core.ForthException>(() => f.EvalAsync("X"));
        }
    }
}
