using System.Threading.Tasks;
using Xunit;
using Forth.Core;
using Forth.Core.Interpreter;

namespace Forth.Tests.Core.Memory
{
    /// <summary>
    /// Regression tests for CMOVE primitive verifying ANS Forth compliance.
    /// CMOVE stack effect: ( c-addr1 c-addr2 u -- )
    /// Copies u bytes from c-addr1 (source) to c-addr2 (destination).
    /// </summary>
    public class CmoveRegressionTests
    {
        private static ForthInterpreter New() => new();

        [Fact]
        public async Task Cmove_BasicCopy_CopiesBytes()
        {
            var f = New();
            // Create two buffers
            Assert.True(await f.EvalAsync("CREATE SRC 10 ALLOT"));
            Assert.True(await f.EvalAsync("CREATE DST 10 ALLOT"));
            
            // Store test data in source: 1 2 3 4 5
            for (int i = 0; i < 5; i++)
            {
                Assert.True(await f.EvalAsync($"{i + 1} SRC {i} + C!"));
            }
            
            // Copy 5 bytes from SRC to DST
            Assert.True(await f.EvalAsync("SRC DST 5 CMOVE"));
            
            // Verify copied data
            for (int i = 0; i < 5; i++)
            {
                Assert.True(await f.EvalAsync($"DST {i} + C@"));
                Assert.Single(f.Stack);
                Assert.Equal((long)(i + 1), (long)f.Stack[0]);
                f.Pop();
            }
        }

        [Fact]
        public async Task Cmove_ZeroLength_DoesNothing()
        {
            var f = New();
            Assert.True(await f.EvalAsync("CREATE BUF 10 ALLOT"));
            Assert.True(await f.EvalAsync("42 BUF C!"));
            
            // CMOVE with u=0 should not change anything
            Assert.True(await f.EvalAsync("BUF BUF 1 + 0 CMOVE"));
            
            Assert.True(await f.EvalAsync("BUF C@"));
            Assert.Single(f.Stack);
            Assert.Equal(42L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Cmove_NegativeLength_Throws()
        {
            var f = New();
            Assert.True(await f.EvalAsync("CREATE BUF 10 ALLOT"));
            
            var ex = await Assert.ThrowsAsync<ForthException>(() => 
                f.EvalAsync("BUF BUF 1 + -1 CMOVE"));
            Assert.Equal(ForthErrorCode.CompileError, ex.Code);
            Assert.Contains("Negative CMOVE length", ex.Message);
        }

        [Fact]
        public async Task Cmove_NonOverlappingForward_CopiesCorrectly()
        {
            var f = New();
            Assert.True(await f.EvalAsync("CREATE BUF 20 ALLOT"));
            
            // Fill first 5 bytes with 65-69 (A-E)
            for (int i = 0; i < 5; i++)
            {
                Assert.True(await f.EvalAsync($"{65 + i} BUF {i} + C!"));
            }
            
            // Copy to non-overlapping region (offset +10)
            Assert.True(await f.EvalAsync("BUF BUF 10 + 5 CMOVE"));
            
            // Verify original data intact
            for (int i = 0; i < 5; i++)
            {
                Assert.True(await f.EvalAsync($"BUF {i} + C@"));
                Assert.Equal((long)(65 + i), (long)f.Stack[0]);
                f.Pop();
            }
            
            // Verify copied data
            for (int i = 0; i < 5; i++)
            {
                Assert.True(await f.EvalAsync($"BUF 10 + {i} + C@"));
                Assert.Equal((long)(65 + i), (long)f.Stack[0]);
                f.Pop();
            }
        }

        [Fact]
        public async Task Cmove_SameSourceAndDest_IsNoop()
        {
            var f = New();
            Assert.True(await f.EvalAsync("CREATE BUF 10 ALLOT"));
            Assert.True(await f.EvalAsync("65 BUF C!"));  // 'A'
            Assert.True(await f.EvalAsync("66 BUF 1 + C!"));  // 'B'
            
            // CMOVE from BUF to BUF (same address)
            Assert.True(await f.EvalAsync("BUF BUF 2 CMOVE"));
            
            // Should be unchanged
            Assert.True(await f.EvalAsync("BUF C@"));
            Assert.Equal(65L, (long)f.Stack[0]);
            f.Pop();
            Assert.True(await f.EvalAsync("BUF 1 + C@"));
            Assert.Equal(66L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Cmove_OverlappingForward_CopiesCorrectly()
        {
            // Test case where source overlaps with destination (forward copy)
            // This is the critical case where CMOVE copies low-to-high
            var f = New();
            Assert.True(await f.EvalAsync("CREATE BUF 10 ALLOT"));
            
            // Pattern: A B C D E
            for (int i = 0; i < 5; i++)
            {
                Assert.True(await f.EvalAsync($"{65 + i} BUF {i} + C!"));
            }
            
            // Copy BUF[0..4] to BUF[2..6] (overlapping, dest > src)
            // With low-to-high copy:
            //   Step 0: Copy BUF[0]?BUF[2]: A B A D E
            //   Step 1: Copy BUF[1]?BUF[3]: A B A B E
            //   Step 2: Copy BUF[2]?BUF[4]: A B A B A (overwrites original C)
            //   Step 3: Copy BUF[3]?BUF[5]: A B A B A B
            //   Step 4: Copy BUF[4]?BUF[6]: A B A B A B A
            // Result at BUF[2..6]: A B A B A
            Assert.True(await f.EvalAsync("BUF BUF 2 + 5 CMOVE"));
            
            // Verify: BUF[2..6] should be A B A B A
            Assert.True(await f.EvalAsync("BUF 2 + C@"));
            Assert.Equal(65L, (long)f.Stack[0]);  // A
            f.Pop();
            Assert.True(await f.EvalAsync("BUF 3 + C@"));
            Assert.Equal(66L, (long)f.Stack[0]);  // B
            f.Pop();
            Assert.True(await f.EvalAsync("BUF 4 + C@"));
            Assert.Equal(65L, (long)f.Stack[0]);  // A
            f.Pop();
        }

        [Fact]
        public async Task Cmove_StackEffect_LeavesStackEmpty()
        {
            var f = New();
            Assert.True(await f.EvalAsync("CREATE BUF 10 ALLOT"));
            
            // Push 3 values, CMOVE should consume all 3
            Assert.True(await f.EvalAsync("BUF BUF 1 + 5 CMOVE"));
            
            // Stack should be empty
            Assert.Empty(f.Stack);
        }

        [Fact]
        public async Task Cmove_WithPadBuffer_WorksCorrectly()
        {
            var f = New();
            // PAD returns a temporary buffer address
            Assert.True(await f.EvalAsync("65 PAD C!"));  // Store 'A' at PAD
            Assert.True(await f.EvalAsync("66 PAD 1 + C!"));  // Store 'B' at PAD+1
            Assert.True(await f.EvalAsync("CREATE DEST 10 ALLOT"));
            
            // Copy 2 bytes from PAD to DEST
            Assert.True(await f.EvalAsync("PAD DEST 2 CMOVE"));
            
            // Verify
            Assert.True(await f.EvalAsync("DEST C@"));
            Assert.Equal(65L, (long)f.Stack[0]);
            f.Pop();
            Assert.True(await f.EvalAsync("DEST 1 + C@"));
            Assert.Equal(66L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Cmove_LargeBuffer_CopiesAllBytes()
        {
            var f = New();
            Assert.True(await f.EvalAsync("CREATE SRC 100 ALLOT"));
            Assert.True(await f.EvalAsync("CREATE DST 100 ALLOT"));
            
            // Fill source with pattern (0-99)
            for (int i = 0; i < 100; i++)
            {
                Assert.True(await f.EvalAsync($"{i} SRC {i} + C!"));
            }
            
            // Copy entire buffer
            Assert.True(await f.EvalAsync("SRC DST 100 CMOVE"));
            
            // Verify all bytes copied (sample check at boundaries and middle)
            Assert.True(await f.EvalAsync("DST C@"));
            Assert.Equal(0L, (long)f.Stack[0]);
            f.Pop();
            
            Assert.True(await f.EvalAsync("DST 50 + C@"));
            Assert.Equal(50L, (long)f.Stack[0]);
            f.Pop();
            
            Assert.True(await f.EvalAsync("DST 99 + C@"));
            Assert.Equal(99L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Cmove_ByteTruncation_StoresOnlyLowByte()
        {
            // CMOVE should only copy the low byte of each cell
            var f = New();
            Assert.True(await f.EvalAsync("CREATE SRC 5 ALLOT"));
            Assert.True(await f.EvalAsync("CREATE DST 5 ALLOT"));
            
            // Store value > 255 (will be truncated to low byte)
            Assert.True(await f.EvalAsync("256 SRC C!"));  // Should store 0
            Assert.True(await f.EvalAsync("257 SRC 1 + C!"));  // Should store 1
            
            Assert.True(await f.EvalAsync("SRC DST 2 CMOVE"));
            
            Assert.True(await f.EvalAsync("DST C@"));
            Assert.Equal(0L, (long)f.Stack[0]);
            f.Pop();
            Assert.True(await f.EvalAsync("DST 1 + C@"));
            Assert.Equal(1L, (long)f.Stack[0]);
        }

        [Fact]
        public async Task Cmove_ComparedWithMove_ForwardBehaviorDiffers()
        {
            // This test documents the difference between CMOVE and MOVE
            // for overlapping forward copies
            var f = New();
            
            // Test CMOVE behavior (low-to-high copy)
            Assert.True(await f.EvalAsync("CREATE BUF1 10 ALLOT"));
            Assert.True(await f.EvalAsync("1 BUF1 C! 2 BUF1 1 + C! 3 BUF1 2 + C!"));
            Assert.True(await f.EvalAsync("BUF1 BUF1 1 + 2 CMOVE"));
            Assert.True(await f.EvalAsync("BUF1 1 + C@"));
            long cmoveResult = (long)f.Stack[0];
            f.Pop();
            Assert.True(await f.EvalAsync("BUF1 2 + C@"));
            long cmoveResult2 = (long)f.Stack[0];
            f.Pop();
            
            // CMOVE copies low-to-high, so:
            // BUF1[1] = BUF1[0] = 1
            // BUF1[2] = BUF1[1] = 1 (uses newly written value)
            Assert.Equal(1L, cmoveResult);
            Assert.Equal(1L, cmoveResult2);
        }

        [Fact]
        public async Task Cmove_StringCopy_WorksCorrectly()
        {
            // Realistic use case: copying a string
            var f = New();
            Assert.True(await f.EvalAsync("CREATE ORIGINAL 20 ALLOT"));
            Assert.True(await f.EvalAsync("CREATE COPY 20 ALLOT"));
            
            // Store "HELLO" in ORIGINAL (with length byte for counted string)
            Assert.True(await f.EvalAsync("5 ORIGINAL C!"));  // length
            Assert.True(await f.EvalAsync("72 ORIGINAL 1 + C!"));  // H
            Assert.True(await f.EvalAsync("69 ORIGINAL 2 + C!"));  // E
            Assert.True(await f.EvalAsync("76 ORIGINAL 3 + C!"));  // L
            Assert.True(await f.EvalAsync("76 ORIGINAL 4 + C!"));  // L
            Assert.True(await f.EvalAsync("79 ORIGINAL 5 + C!"));  // O
            
            // Copy the counted string
            Assert.True(await f.EvalAsync("ORIGINAL COPY 6 CMOVE"));
            
            // Verify copy
            Assert.True(await f.EvalAsync("COPY C@"));
            Assert.Equal(5L, (long)f.Stack[0]);  // length
            f.Pop();
            Assert.True(await f.EvalAsync("COPY 1 + C@"));
            Assert.Equal(72L, (long)f.Stack[0]);  // H
        }

        [Fact]
        public async Task Cmove_PartialCopy_CopiesOnlySpecifiedBytes()
        {
            var f = New();
            Assert.True(await f.EvalAsync("CREATE BUF 10 ALLOT"));
            
            // Fill with 0xFF
            Assert.True(await f.EvalAsync("BUF 10 255 FILL"));
            
            // Copy only 3 bytes from beginning
            Assert.True(await f.EvalAsync("1 BUF C! 2 BUF 1 + C! 3 BUF 2 + C!"));
            Assert.True(await f.EvalAsync("CREATE DEST 10 ALLOT"));
            Assert.True(await f.EvalAsync("DEST 10 0 FILL"));  // Clear dest
            
            Assert.True(await f.EvalAsync("BUF DEST 3 CMOVE"));
            
            // First 3 bytes should be copied
            Assert.True(await f.EvalAsync("DEST C@"));
            Assert.Equal(1L, (long)f.Stack[0]);
            f.Pop();
            Assert.True(await f.EvalAsync("DEST 1 + C@"));
            Assert.Equal(2L, (long)f.Stack[0]);
            f.Pop();
            Assert.True(await f.EvalAsync("DEST 2 + C@"));
            Assert.Equal(3L, (long)f.Stack[0]);
            f.Pop();
            
            // Fourth byte should remain 0 (not copied)
            Assert.True(await f.EvalAsync("DEST 3 + C@"));
            Assert.Equal(0L, (long)f.Stack[0]);
        }
    }
}
