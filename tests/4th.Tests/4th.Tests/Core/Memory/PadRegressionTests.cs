using System.Threading.Tasks;
using Xunit;
using Forth.Core;
using Forth.Core.Interpreter;

namespace Forth.Tests.Core.Memory
{
    /// <summary>
    /// Regression tests for PAD primitive verifying ANS Forth compliance.
    /// PAD should return a stable address that doesn't change when dictionary grows.
    /// 
    /// Bug Fix: PAD was previously implemented as (_nextAddr + 256), causing its address
    /// to change as the dictionary grew. This violated ANS Forth semantics.
    /// 
    /// Fix: PAD now returns a fixed address (900000L) that remains stable regardless
    /// of dictionary modifications.
    /// </summary>
    public class PadRegressionTests
    {
        private static ForthInterpreter New() => new();

        [Fact]
        public async Task PAD_ReturnsStableAddress_AcrossDictionaryGrowth()
        {
            // This is the core regression test for the bug fix
            // Previously, PAD would return different addresses as dictionary grew
            var f = New();
            
            // Get initial PAD address
            Assert.True(await f.EvalAsync("PAD"));
            var pad1 = (long)f.Pop();
            
            // Grow dictionary with CREATE and ALLOT
            Assert.True(await f.EvalAsync("CREATE TEST1 10 ALLOT"));
            
            // PAD address should remain the same
            Assert.True(await f.EvalAsync("PAD"));
            var pad2 = (long)f.Pop();
            Assert.Equal(pad1, pad2);
            
            // Grow dictionary more
            Assert.True(await f.EvalAsync("CREATE TEST2 100 ALLOT"));
            
            // PAD address should still be the same
            Assert.True(await f.EvalAsync("PAD"));
            var pad3 = (long)f.Pop();
            Assert.Equal(pad1, pad3);
        }

        [Fact]
        public async Task PAD_DataPersists_AcrossDictionaryGrowth()
        {
            // Verify that data stored at PAD persists when dictionary grows
            // This was the failing scenario that exposed the bug
            var f = New();
            
            // Store test data at PAD
            Assert.True(await f.EvalAsync("65 PAD C!"));      // 'A'
            Assert.True(await f.EvalAsync("66 PAD 1 + C!"));  // 'B'
            Assert.True(await f.EvalAsync("67 PAD 2 + C!"));  // 'C'
            
            // Grow dictionary (this would have moved PAD in the buggy version)
            Assert.True(await f.EvalAsync("CREATE TEST 50 ALLOT"));
            
            // Data at PAD should still be accessible at the same addresses
            Assert.True(await f.EvalAsync("PAD C@"));
            Assert.Equal(65L, (long)f.Pop());
            
            Assert.True(await f.EvalAsync("PAD 1 + C@"));
            Assert.Equal(66L, (long)f.Pop());
            
            Assert.True(await f.EvalAsync("PAD 2 + C@"));
            Assert.Equal(67L, (long)f.Pop());
        }

        [Fact]
        public async Task PAD_IsAboveHERE_Initially()
        {
            // PAD should be above the dictionary space
            // This ensures PAD doesn't conflict with dictionary allocations
            var f = New();
            
            Assert.True(await f.EvalAsync("HERE PAD"));
            var here = (long)f.Stack[0];
            var pad = (long)f.Stack[1];
            
            Assert.True(pad > here, $"PAD ({pad}) should be above HERE ({here})");
        }

        [Fact]
        public async Task PAD_IsAboveHERE_AfterGrowth()
        {
            // PAD should remain above HERE even after dictionary growth
            var f = New();
            
            // Grow dictionary significantly
            Assert.True(await f.EvalAsync("CREATE BIG 1000 ALLOT"));
            
            Assert.True(await f.EvalAsync("HERE PAD"));
            var here = (long)f.Stack[0];
            var pad = (long)f.Stack[1];
            
            Assert.True(pad > here, $"PAD ({pad}) should still be above HERE ({here}) after growth");
        }

        [Fact]
        public async Task PAD_MultipleReads_ReturnSameAddress()
        {
            // Multiple calls to PAD should return the same address
            var f = New();
            
            Assert.True(await f.EvalAsync("PAD PAD PAD"));
            var pad1 = (long)f.Pop();
            var pad2 = (long)f.Pop();
            var pad3 = (long)f.Pop();
            
            Assert.Equal(pad1, pad2);
            Assert.Equal(pad2, pad3);
        }

        [Fact]
        public async Task PAD_CanBeUsedWithCMOVE()
        {
            // Verify PAD works correctly with CMOVE after dictionary growth
            // This is the exact scenario from the failing test that exposed the bug
            var f = New();
            
            // Store data at PAD
            Assert.True(await f.EvalAsync("72 PAD C!"));      // 'H'
            Assert.True(await f.EvalAsync("73 PAD 1 + C!"));  // 'I'
            
            // Create destination buffer (grows dictionary)
            Assert.True(await f.EvalAsync("CREATE DEST 10 ALLOT"));
            
            // Copy from PAD to DEST (this failed before the fix)
            Assert.True(await f.EvalAsync("PAD DEST 2 CMOVE"));
            
            // Verify copied data
            Assert.True(await f.EvalAsync("DEST C@"));
            Assert.Equal(72L, (long)f.Pop());
            
            Assert.True(await f.EvalAsync("DEST 1 + C@"));
            Assert.Equal(73L, (long)f.Pop());
        }

        [Fact]
        public async Task PAD_HasSufficientSpace()
        {
            // Verify PAD has sufficient space for typical usage
            // ANS Forth doesn't specify PAD size, but 128+ bytes is common
            var f = New();
            
            // Fill PAD with test data (128 bytes)
            for (int i = 0; i < 128; i++)
            {
                Assert.True(await f.EvalAsync($"{i % 256} PAD {i} + C!"));
            }
            
            // Verify all data is accessible
            for (int i = 0; i < 128; i++)
            {
                Assert.True(await f.EvalAsync($"PAD {i} + C@"));
                Assert.Equal((long)(i % 256), (long)f.Pop());
            }
        }

        [Fact]
        public async Task PAD_IsBelowHeap()
        {
            // PAD should be below heap space (starts at 1000000)
            // This ensures PAD doesn't conflict with ALLOCATE/FREE
            var f = New();
            
            Assert.True(await f.EvalAsync("PAD"));
            var pad = (long)f.Pop();
            
            Assert.True(pad < 1000000L, $"PAD ({pad}) should be below heap space (1000000)");
        }

        [Fact]
        public async Task PAD_CanStoreAndRetrieveCountedString()
        {
            // Test typical PAD usage pattern: building counted strings
            var f = New();
            
            // Build a counted string "HELLO" at PAD
            Assert.True(await f.EvalAsync("5 PAD C!"));       // length
            Assert.True(await f.EvalAsync("72 PAD 1 + C!"));  // H
            Assert.True(await f.EvalAsync("69 PAD 2 + C!"));  // E
            Assert.True(await f.EvalAsync("76 PAD 3 + C!"));  // L
            Assert.True(await f.EvalAsync("76 PAD 4 + C!"));  // L
            Assert.True(await f.EvalAsync("79 PAD 5 + C!"));  // O
            
            // Grow dictionary
            Assert.True(await f.EvalAsync("CREATE DUMMY 20 ALLOT"));
            
            // Verify counted string is still intact at PAD
            Assert.True(await f.EvalAsync("PAD C@"));
            Assert.Equal(5L, (long)f.Pop()); // length
            
            Assert.True(await f.EvalAsync("PAD 1 + C@"));
            Assert.Equal(72L, (long)f.Pop()); // H
            
            Assert.True(await f.EvalAsync("PAD 5 + C@"));
            Assert.Equal(79L, (long)f.Pop()); // O
        }

        [Fact]
        public async Task PAD_MultipleDictionaryOperations_StableAddress()
        {
            // Comprehensive test: multiple dictionary operations shouldn't affect PAD
            var f = New();
            
            // Get initial PAD address
            Assert.True(await f.EvalAsync("PAD"));
            var initialPad = (long)f.Pop();
            
            // Store marker value at PAD
            Assert.True(await f.EvalAsync("123 PAD !"));
            
            // Perform various dictionary operations
            Assert.True(await f.EvalAsync("VARIABLE V1"));
            Assert.True(await f.EvalAsync("100 ALLOT"));
            Assert.True(await f.EvalAsync(": TEST1 1 2 + ;"));
            Assert.True(await f.EvalAsync("42 CONSTANT V2"));
            Assert.True(await f.EvalAsync("50 ALLOT"));
            
            // PAD address should be unchanged
            Assert.True(await f.EvalAsync("PAD"));
            var finalPad = (long)f.Pop();
            Assert.Equal(initialPad, finalPad);
            
            // Marker value should still be at PAD
            Assert.True(await f.EvalAsync("PAD @"));
            Assert.Equal(123L, (long)f.Pop());
        }

        [Fact]
        public async Task PAD_WorksWithFILL()
        {
            // Test PAD with FILL primitive
            var f = New();
            
            // Fill PAD with 'X' (88)
            Assert.True(await f.EvalAsync("PAD 10 88 FILL"));
            
            // Verify filled data
            for (int i = 0; i < 10; i++)
            {
                Assert.True(await f.EvalAsync($"PAD {i} + C@"));
                Assert.Equal(88L, (long)f.Pop());
            }
            
            // Grow dictionary
            Assert.True(await f.EvalAsync("CREATE TEST 30 ALLOT"));
            
            // Data at PAD should still be intact
            Assert.True(await f.EvalAsync("PAD C@"));
            Assert.Equal(88L, (long)f.Pop());
        }

        [Fact]
        public async Task PAD_AddressMatchesInternalPadAddr()
        {
            // Verify PAD primitive returns the internal _padAddr field value
            var f = New();
            
            Assert.True(await f.EvalAsync("PAD"));
            var padFromPrimitive = (long)f.Pop();
            
            // Internal field should be 900000L as per implementation
            Assert.Equal(900000L, padFromPrimitive);
            
            // Also verify it's accessible via the PadAddr property
            Assert.Equal(900000L, f.PadAddr);
        }
    }
}
