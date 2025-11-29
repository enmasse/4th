using Xunit;
using Forth.Core.Interpreter;
using System.Threading.Tasks;
using System.IO;
using Forth.Core;
using System.Collections.Generic;

namespace Forth.Tests.Core.MissingWords;

public class BlockSystemTests
{
    [Fact]
    public async Task BlockLoadAndSave()
    {
        var f = new ForthInterpreter();
        // Create a block buffer via WORDLIST and set definitions directly
        Assert.True(await f.EvalAsync("WORDLIST DEFINITIONS"));
        // Use simplified ACCEPT to get a string and then SAVE to block 1
        Assert.True(await f.EvalAsync("S\" hello\" 5 1 SAVE"));
        Assert.True(await f.EvalAsync("1 BLOCK"));
        // BLK should push current block number
        Assert.True(await f.EvalAsync("BLK"));
        var blk = (int)(long)f.Stack[^1];
        Assert.Equal(1, blk);
    }

    private sealed class TestIO : IForthIO
    {
        public readonly List<string> Outputs = new();
        public void Print(string text) => Outputs.Add(text);
        public void PrintNumber(long number) => Outputs.Add(number.ToString());
        public void NewLine() => Outputs.Add("\n");
        public string? ReadLine() => null;
    }

    [Fact]
    public async Task ListBlock()
    {
        var io = new TestIO();
        var f = new ForthInterpreter(io);
        // Save some content to block 0
        await f.EvalAsync("S\"Line 1 content\" 0 SAVE");
        // List block 0
        await f.EvalAsync("0 LIST");
        var result = string.Join("", io.Outputs);
        Assert.Contains("00 Line 1 content", result);
        // Other lines should be empty
        for (int i = 1; i < 16; i++)
        {
            Assert.Contains($"{i:D2} ", result);
        }
    }

    [Fact]
    public async Task BufferAssignsBlockBuffer()
    {
        var f = new ForthInterpreter();
        // BUFFER should assign a buffer for block 5 and return address
        Assert.True(await f.EvalAsync("5 BUFFER"));
        Assert.True(f.Stack.Count == 1);
        var addr = (long)f.Stack[0];
        Assert.True(addr > 0);
        // Contents should be undefined, but in this impl, initialized to zero
        f.MemTryGet(addr, out var val);
        Assert.Equal(0, val);
    }

    [Fact]
    public async Task EmptyBuffersClearsAllBuffers()
    {
        var f = new ForthInterpreter();
        // Assign buffers for a few blocks
        await f.EvalAsync("1 BUFFER DROP");
        await f.EvalAsync("2 BUFFER DROP");
        await f.EvalAsync("3 BUFFER DROP");
        Assert.Equal(3, f.BlockMappingCount);
        // Empty buffers
        await f.EvalAsync("EMPTY-BUFFERS");
        Assert.Equal(0, f.BlockMappingCount);
    }

    [Fact]
    public async Task ScrVariableHoldsLastListedBlock()
    {
        var f = new ForthInterpreter();
        // Save content to block 5
        await f.EvalAsync("S\" test content\" 5 SAVE");
        // List block 5
        await f.EvalAsync("5 LIST");
        // SCR should hold 5
        await f.EvalAsync("SCR @");
        Assert.Single(f.Stack);
        Assert.Equal(5L, (long)f.Stack[0]);
    }

    [Fact]
    public async Task UpdateMarksCurrentBlockDirty()
    {
        var f = new ForthInterpreter();
        // Set current block to 10
        await f.EvalAsync("10 BLOCK DROP");
        // Update should mark it dirty
        await f.EvalAsync("UPDATE");
        // Dirty blocks should contain 10
        Assert.Contains(10, f._dirtyBlocks);
    }

    [Fact]
    public async Task SaveBuffersSavesDirtyBlocks()
    {
        var f = new ForthInterpreter();
        // Set current block to 20
        await f.EvalAsync("20 BLOCK DROP");
        // Update to mark dirty
        await f.EvalAsync("UPDATE");
        Assert.Contains(20, f._dirtyBlocks);
        // SAVE-BUFFERS should clear dirty
        await f.EvalAsync("SAVE-BUFFERS");
        Assert.DoesNotContain(20, f._dirtyBlocks);
    }

    [Fact]
    public async Task FlushSavesAndClearsBuffers()
    {
        var f = new ForthInterpreter();
        // Assign buffer for block 30
        await f.EvalAsync("30 BUFFER DROP");
        Assert.Equal(1, f.BlockMappingCount);
        // Set current to 30, update
        await f.EvalAsync("30 BLOCK DROP UPDATE");
        Assert.Contains(30, f._dirtyBlocks);
        // FLUSH should save and clear buffers
        await f.EvalAsync("FLUSH");
        Assert.DoesNotContain(30, f._dirtyBlocks);
        Assert.Equal(0, f.BlockMappingCount);
    }
}
