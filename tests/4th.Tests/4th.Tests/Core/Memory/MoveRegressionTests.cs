using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core.Memory;

/// <summary>
/// Comprehensive regression tests for the MOVE primitive.
/// MOVE ( src dst u -- ) copies u bytes from src to dst, handling overlapping regions correctly.
/// </summary>
public class MoveRegressionTests
{
    private static ForthInterpreter New() => new();

    /// <summary>
    /// Test basic non-overlapping copy from source to destination.
    /// Expected: All bytes copied correctly.
    /// </summary>
    [Fact]
    public async Task Move_BasicCopy_NonOverlapping()
    {
        var f = New();
        // Create two buffers: SRC and DST
        Assert.True(await f.EvalAsync("CREATE SRC 10 ALLOT CREATE DST 10 ALLOT"));
        
        // Fill SRC with values 1-10
        Assert.True(await f.EvalAsync(": FILL-SRC 10 0 DO I 1+ SRC I + C! LOOP ;"));
        Assert.True(await f.EvalAsync("FILL-SRC"));
        
        // Move 10 bytes from SRC to DST
        Assert.True(await f.EvalAsync("SRC DST 10 MOVE"));
        
        // Verify all 10 bytes copied
        Assert.True(await f.EvalAsync("DST C@ DST 1 + C@ DST 2 + C@ DST 3 + C@ DST 4 + C@"));
        Assert.Equal(5, f.Stack.Count);
        Assert.Equal(1L, (long)f.Stack[0]);
        Assert.Equal(2L, (long)f.Stack[1]);
        Assert.Equal(3L, (long)f.Stack[2]);
        Assert.Equal(4L, (long)f.Stack[3]);
        Assert.Equal(5L, (long)f.Stack[4]);
    }

    /// <summary>
    /// Test zero-length MOVE (should be no-op).
    /// Expected: No bytes copied, no error.
    /// </summary>
    [Fact]
    public async Task Move_ZeroLength_NoOp()
    {
        var f = New();
        Assert.True(await f.EvalAsync("CREATE BUF 5 ALLOT"));
        Assert.True(await f.EvalAsync("42 BUF C!"));
        
        // Move zero bytes
        Assert.True(await f.EvalAsync("BUF BUF 1 + 0 MOVE"));
        
        // Original value should be unchanged
        Assert.True(await f.EvalAsync("BUF C@"));
        Assert.Single(f.Stack);
        Assert.Equal(42L, (long)f.Stack[0]);
    }

    /// <summary>
    /// Test overlapping copy where dst > src (forward overlap).
    /// MOVE should handle this by copying from high to low addresses.
    /// Expected: Correct copy without corruption.
    /// </summary>
    [Fact]
    public async Task Move_OverlappingForward_CopiesCorrectly()
    {
        var f = New();
        Assert.True(await f.EvalAsync("CREATE BUF 10 ALLOT"));
        
        // Fill with values 65 ('A'), 66 ('B'), 67 ('C')
        Assert.True(await f.EvalAsync("65 BUF C! 66 BUF 1 + C! 67 BUF 2 + C!"));
        
        // Move 3 bytes from BUF to BUF+1 (overlap: dst > src)
        Assert.True(await f.EvalAsync("BUF BUF 1 + 3 MOVE"));
        
        // Result should be: A, A, B, C
        Assert.True(await f.EvalAsync("BUF C@ BUF 1 + C@ BUF 2 + C@ BUF 3 + C@"));
        Assert.Equal(4, f.Stack.Count);
        Assert.Equal(65L, (long)f.Stack[0]); // 'A'
        Assert.Equal(65L, (long)f.Stack[1]); // 'A'
        Assert.Equal(66L, (long)f.Stack[2]); // 'B'
        Assert.Equal(67L, (long)f.Stack[3]); // 'C'
    }

    /// <summary>
    /// Test overlapping copy where dst &lt; src (backward overlap).
    /// MOVE should handle this by copying from low to high addresses.
    /// Expected: Correct copy without corruption.
    /// </summary>
    [Fact]
    public async Task Move_OverlappingBackward_CopiesCorrectly()
    {
        var f = New();
        Assert.True(await f.EvalAsync("CREATE BUF 10 ALLOT"));
        
        // Fill with values at offset positions
        Assert.True(await f.EvalAsync("65 BUF 2 + C! 66 BUF 3 + C! 67 BUF 4 + C!"));
        
        // Move 3 bytes from BUF+2 to BUF (overlap: dst < src)
        Assert.True(await f.EvalAsync("BUF 2 + BUF 3 MOVE"));
        
        // Result should be: A, B, C at positions 0, 1, 2
        Assert.True(await f.EvalAsync("BUF C@ BUF 1 + C@ BUF 2 + C@"));
        Assert.Equal(3, f.Stack.Count);
        Assert.Equal(65L, (long)f.Stack[0]); // 'A'
        Assert.Equal(66L, (long)f.Stack[1]); // 'B'
        Assert.Equal(67L, (long)f.Stack[2]); // 'C'
    }

    /// <summary>
    /// Test copying to same address (src == dst).
    /// Expected: No change, no error.
    /// </summary>
    [Fact]
    public async Task Move_SameAddress_NoChange()
    {
        var f = New();
        Assert.True(await f.EvalAsync("CREATE BUF 5 ALLOT"));
        Assert.True(await f.EvalAsync("1 BUF C! 2 BUF 1 + C! 3 BUF 2 + C!"));
        
        // Move to same address
        Assert.True(await f.EvalAsync("BUF BUF 3 MOVE"));
        
        // Values should be unchanged
        Assert.True(await f.EvalAsync("BUF C@ BUF 1 + C@ BUF 2 + C@"));
        Assert.Equal(3, f.Stack.Count);
        Assert.Equal(1L, (long)f.Stack[0]);
        Assert.Equal(2L, (long)f.Stack[1]);
        Assert.Equal(3L, (long)f.Stack[2]);
    }

    /// <summary>
    /// Test single byte MOVE.
    /// Expected: Single byte copied correctly.
    /// </summary>
    [Fact]
    public async Task Move_SingleByte_CopiesCorrectly()
    {
        var f = New();
        Assert.True(await f.EvalAsync("CREATE SRC 1 ALLOT CREATE DST 1 ALLOT"));
        Assert.True(await f.EvalAsync("99 SRC C!"));
        
        // Move 1 byte
        Assert.True(await f.EvalAsync("SRC DST 1 MOVE"));
        
        Assert.True(await f.EvalAsync("DST C@"));
        Assert.Single(f.Stack);
        Assert.Equal(99L, (long)f.Stack[0]);
    }

    /// <summary>
    /// Test large MOVE operation.
    /// Expected: All bytes copied correctly.
    /// </summary>
    [Fact]
    public async Task Move_LargeBuffer_CopiesCorrectly()
    {
        var f = New();
        Assert.True(await f.EvalAsync("CREATE SRC 100 ALLOT CREATE DST 100 ALLOT"));
        
        // Fill source with pattern
        Assert.True(await f.EvalAsync(": FILL-PATTERN 100 0 DO I 256 MOD SRC I + C! LOOP ;"));
        Assert.True(await f.EvalAsync("FILL-PATTERN"));
        
        // Move 100 bytes
        Assert.True(await f.EvalAsync("SRC DST 100 MOVE"));
        
        // Verify first, middle, and last bytes
        Assert.True(await f.EvalAsync("DST C@ DST 50 + C@ DST 99 + C@"));
        Assert.Equal(3, f.Stack.Count);
        Assert.Equal(0L, (long)f.Stack[0]);
        Assert.Equal(50L, (long)f.Stack[1]);
        Assert.Equal(99L, (long)f.Stack[2]);
    }

    /// <summary>
    /// Test negative length MOVE (should throw error).
    /// Expected: ForthException thrown.
    /// </summary>
    [Fact]
    public async Task Move_NegativeLength_ThrowsError()
    {
        var f = New();
        Assert.True(await f.EvalAsync("CREATE BUF 10 ALLOT"));
        
        await Assert.ThrowsAsync<ForthException>(async () =>
            await f.EvalAsync("BUF BUF 1 + -1 MOVE"));
    }

    /// <summary>
    /// Test MOVE with byte values at boundaries (0 and 255).
    /// Expected: Boundary values copied correctly.
    /// </summary>
    [Fact]
    public async Task Move_BoundaryValues_CopiesCorrectly()
    {
        var f = New();
        Assert.True(await f.EvalAsync("CREATE SRC 3 ALLOT CREATE DST 3 ALLOT"));
        
        // Store boundary values: 0, 255, 128
        Assert.True(await f.EvalAsync("0 SRC C! 255 SRC 1 + C! 128 SRC 2 + C!"));
        
        // Move 3 bytes
        Assert.True(await f.EvalAsync("SRC DST 3 MOVE"));
        
        Assert.True(await f.EvalAsync("DST C@ DST 1 + C@ DST 2 + C@"));
        Assert.Equal(3, f.Stack.Count);
        Assert.Equal(0L, (long)f.Stack[0]);
        Assert.Equal(255L, (long)f.Stack[1]);
        Assert.Equal(128L, (long)f.Stack[2]);
    }

    /// <summary>
    /// Test MOVE preserves bytes as bytes (masks to 8 bits).
    /// Expected: Only low byte of each cell is copied.
    /// </summary>
    [Fact]
    public async Task Move_PreservesBytes_MasksTo8Bits()
    {
        var f = New();
        Assert.True(await f.EvalAsync("CREATE SRC 2 ALLOT CREATE DST 2 ALLOT"));
        
        // Store values that would be larger than bytes if not masked
        Assert.True(await f.EvalAsync("256 SRC C! 257 SRC 1 + C!"));
        
        // Move 2 bytes
        Assert.True(await f.EvalAsync("SRC DST 2 MOVE"));
        
        // Should read back as 0 and 1 (masked)
        Assert.True(await f.EvalAsync("DST C@ DST 1 + C@"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(0L, (long)f.Stack[0]);
        Assert.Equal(1L, (long)f.Stack[1]);
    }

    /// <summary>
    /// Test MOVE from PAD to HERE region (common Forth idiom).
    /// Expected: Bytes copied correctly between dictionary and scratch pad.
    /// </summary>
    [Fact]
    public async Task Move_PadToHere_CopiesCorrectly()
    {
        var f = New();
        // Allocate buffer at HERE
        Assert.True(await f.EvalAsync("HERE CONSTANT BUF 10 ALLOT"));
        
        // Write pattern to PAD
        Assert.True(await f.EvalAsync("65 PAD C! 66 PAD 1 + C! 67 PAD 2 + C!"));
        
        // Move 3 bytes from PAD to BUF
        Assert.True(await f.EvalAsync("PAD BUF 3 MOVE"));
        
        // Verify
        Assert.True(await f.EvalAsync("BUF C@ BUF 1 + C@ BUF 2 + C@"));
        Assert.Equal(3, f.Stack.Count);
        Assert.Equal(65L, (long)f.Stack[0]);
        Assert.Equal(66L, (long)f.Stack[1]);
        Assert.Equal(67L, (long)f.Stack[2]);
    }

    /// <summary>
    /// Test MOVE vs CMOVE behavior (MOVE should handle overlaps, CMOVE forward only).
    /// This verifies MOVE's overlap handling is distinct from CMOVE.
    /// Expected: MOVE handles forward overlap correctly.
    /// </summary>
    [Fact]
    public async Task Move_VsCMove_OverlapHandling()
    {
        var f = New();
        Assert.True(await f.EvalAsync("CREATE BUF 20 ALLOT"));
        
        // Test setup: fill with sequence
        Assert.True(await f.EvalAsync(": FILL-SEQ 10 0 DO I BUF I + C! LOOP ;"));
        Assert.True(await f.EvalAsync("FILL-SEQ"));
        
        // Move 5 bytes forward (BUF to BUF+3) with MOVE
        Assert.True(await f.EvalAsync("BUF BUF 3 + 5 MOVE"));
        
        // Check result - should preserve data correctly
        Assert.True(await f.EvalAsync("BUF 3 + C@ BUF 4 + C@ BUF 5 + C@"));
        Assert.Equal(3, f.Stack.Count);
        Assert.Equal(0L, (long)f.Stack[0]);
        Assert.Equal(1L, (long)f.Stack[1]);
        Assert.Equal(2L, (long)f.Stack[2]);
    }

    /// <summary>
    /// Test MOVE with exact overlap boundary (src+u == dst).
    /// Expected: Correct copy at exact boundary.
    /// </summary>
    [Fact]
    public async Task Move_ExactBoundaryOverlap_CopiesCorrectly()
    {
        var f = New();
        Assert.True(await f.EvalAsync("CREATE BUF 10 ALLOT"));
        
        // Fill positions 0-4 with values
        Assert.True(await f.EvalAsync("1 BUF C! 2 BUF 1 + C! 3 BUF 2 + C! 4 BUF 3 + C! 5 BUF 4 + C!"));
        
        // Move 5 bytes where src+u == dst (BUF to BUF+5)
        Assert.True(await f.EvalAsync("BUF BUF 5 + 5 MOVE"));
        
        // Positions 5-9 should have 1-5
        Assert.True(await f.EvalAsync("BUF 5 + C@ BUF 6 + C@ BUF 7 + C@ BUF 8 + C@ BUF 9 + C@"));
        Assert.Equal(5, f.Stack.Count);
        Assert.Equal(1L, (long)f.Stack[0]);
        Assert.Equal(2L, (long)f.Stack[1]);
        Assert.Equal(3L, (long)f.Stack[2]);
        Assert.Equal(4L, (long)f.Stack[3]);
        Assert.Equal(5L, (long)f.Stack[4]);
    }

    /// <summary>
    /// Test MOVE with partial overlap at end of source.
    /// Expected: Correct handling of partial overlap.
    /// </summary>
    [Fact]
    public async Task Move_PartialOverlapAtEnd_CopiesCorrectly()
    {
        var f = New();
        Assert.True(await f.EvalAsync("CREATE BUF 20 ALLOT"));
        
        // Setup: write pattern
        Assert.True(await f.EvalAsync(": SETUP 10 0 DO I 1+ BUF I + C! LOOP ;"));
        Assert.True(await f.EvalAsync("SETUP"));
        
        // Move 6 bytes from BUF+4 to BUF+2 (partial overlap)
        Assert.True(await f.EvalAsync("BUF 4 + BUF 2 + 6 MOVE"));
        
        // Check positions 2-7 now have values 5-10
        Assert.True(await f.EvalAsync("BUF 2 + C@ BUF 3 + C@ BUF 4 + C@ BUF 5 + C@"));
        Assert.Equal(4, f.Stack.Count);
        Assert.Equal(5L, (long)f.Stack[0]);
        Assert.Equal(6L, (long)f.Stack[1]);
        Assert.Equal(7L, (long)f.Stack[2]);
        Assert.Equal(8L, (long)f.Stack[3]);
    }

    /// <summary>
    /// Test MOVE in a defined word (compilation mode).
    /// Expected: MOVE works correctly when compiled.
    /// </summary>
    [Fact]
    public async Task Move_InDefinedWord_WorksCorrectly()
    {
        var f = New();
        Assert.True(await f.EvalAsync("CREATE SRC 5 ALLOT CREATE DST 5 ALLOT"));
        Assert.True(await f.EvalAsync(": TEST-MOVE 10 SRC C! 20 SRC 1 + C! SRC DST 2 MOVE ;"));
        
        Assert.True(await f.EvalAsync("TEST-MOVE"));
        
        Assert.True(await f.EvalAsync("DST C@ DST 1 + C@"));
        Assert.Equal(2, f.Stack.Count);
        Assert.Equal(10L, (long)f.Stack[0]);
        Assert.Equal(20L, (long)f.Stack[1]);
    }

    /// <summary>
    /// Test MOVE with string data (common use case).
    /// Expected: String bytes copied correctly.
    /// </summary>
    [Fact]
    public async Task Move_StringData_CopiesCorrectly()
    {
        var f = New();
        Assert.True(await f.EvalAsync("CREATE DEST 20 ALLOT"));
        
        // Use S" to create a string and get its address
        Assert.True(await f.EvalAsync("S\" HELLO\" DROP"));
        var strAddr = (long)f.Pop();
        
        // Move 5 characters
        Assert.True(await f.EvalAsync($"{strAddr} DEST 5 MOVE"));
        
        // Verify characters
        Assert.True(await f.EvalAsync("DEST C@ DEST 1 + C@ DEST 2 + C@ DEST 3 + C@ DEST 4 + C@"));
        Assert.Equal(5, f.Stack.Count);
        Assert.Equal(72L, (long)f.Stack[0]); // 'H'
        Assert.Equal(69L, (long)f.Stack[1]); // 'E'
        Assert.Equal(76L, (long)f.Stack[2]); // 'L'
        Assert.Equal(76L, (long)f.Stack[3]); // 'L'
        Assert.Equal(79L, (long)f.Stack[4]); // 'O'
    }

    /// <summary>
    /// Test interaction between MOVE and FILL.
    /// Expected: FILL then MOVE operates correctly.
    /// </summary>
    [Fact]
    public async Task Move_AfterFill_PreservesFilledData()
    {
        var f = New();
        Assert.True(await f.EvalAsync("CREATE BUF1 10 ALLOT CREATE BUF2 10 ALLOT"));
        
        // Fill BUF1 with 'X' (88)
        Assert.True(await f.EvalAsync("BUF1 10 88 FILL"));
        
        // Move to BUF2
        Assert.True(await f.EvalAsync("BUF1 BUF2 10 MOVE"));
        
        // Verify BUF2 has 'X's
        Assert.True(await f.EvalAsync("BUF2 C@ BUF2 5 + C@ BUF2 9 + C@"));
        Assert.Equal(3, f.Stack.Count);
        Assert.Equal(88L, (long)f.Stack[0]);
        Assert.Equal(88L, (long)f.Stack[1]);
        Assert.Equal(88L, (long)f.Stack[2]);
    }
}
