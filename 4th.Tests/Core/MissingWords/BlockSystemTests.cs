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
}
