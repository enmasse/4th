using System;
using Forth.Core.Interpreter;
using System.Threading.Tasks;
using System.IO;
using System.Text;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive("BLOCK", HelpString = "BLOCK ( n -- c-addr u ) load block n")]
    private static Task Prim_BLOCK(ForthInterpreter i)
    {
        i.EnsureStack(1, "BLOCK");
        var n = (int)ToLong(i.PopInternal());
        i.EnsureBlockExistsOnDisk(n);
        var addr = i.GetOrAllocateBlockAddr(n);
        i.LoadBlockFromBacking(n, addr);
        i.SetCurrentBlockNumber(n);
        i.Push((long)addr);
        i.Push((long)ForthInterpreter.BlockSize);
        return Task.CompletedTask;
    }

    [Primitive("SAVE", HelpString = "SAVE ( c-addr u n -- ) save bytes to block n")]
    private static Task Prim_SAVE(ForthInterpreter i)
    {
        i.EnsureStack(3, "SAVE");
        var n = (int)ToLong(i.PopInternal());
        var u = (int)ToLong(i.PopInternal());
        var src = i.PopInternal();
        if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative length");
        if (u > ForthInterpreter.BlockSize) u = ForthInterpreter.BlockSize;
        if (src is string s)
        {
            var content = s.Length > u ? s.Substring(0, u) : s.PadRight(u, ' ');
            i.SetBlockBuffer(n, content);
            return Task.CompletedTask;
        }
        if (src is long addr)
        {
            i.SaveBlockToBacking(n, addr, u);
            return Task.CompletedTask;
        }
        throw new ForthException(ForthErrorCode.TypeError, "SAVE expects address or string");
    }

    [Primitive("LIST", HelpString = "LIST ( n -- ) display block n")]
    private static Task Prim_LIST(ForthInterpreter i)
    {
        i.EnsureStack(1, "LIST");
        var n = (int)ToLong(i.PopInternal());
        i.EnsureBlockExistsOnDisk(n);
        var addr = i.GetOrAllocateBlockAddr(n);
        i.LoadBlockFromBacking(n, addr);
        i.SetCurrentBlockNumber(n);
        i._mem[i.ScrAddr] = n;
        var sb = new StringBuilder(ForthInterpreter.BlockSize);
        for (int k = 0; k < ForthInterpreter.BlockSize; k++)
        {
            i.MemTryGet(addr + k, out var v);
            sb.Append((char)(v & 0xFF));
        }
        var blockContent = sb.ToString();
        for (int line = 0; line < 16; line++)
        {
            var start = line * 64;
            var len = Math.Min(64, blockContent.Length - start);
            var lineContent = blockContent.Substring(start, len).TrimEnd('\0');
            i.WriteText($"{line:D2} {lineContent}");
            i.NewLine();
        }
        return Task.CompletedTask;
    }

    [Primitive("LOAD", HelpString = "LOAD ( n -- ) load and interpret block n")]
    private static async Task Prim_LOAD(ForthInterpreter i)
    {
        i.EnsureStack(1, "LOAD");
        var n = (int)ToLong(i.PopInternal());
        i.EnsureBlockExistsOnDisk(n);
        var addr = i.GetOrAllocateBlockAddr(n);
        i.LoadBlockFromBacking(n, addr);
        i.SetCurrentBlockNumber(n);
        var sb = new StringBuilder(ForthInterpreter.BlockSize);
        for (int k = 0; k < ForthInterpreter.BlockSize; k++)
        {
            i.MemTryGet(addr + k, out var v);
            sb.Append((char)(v & 0xFF));
        }
        var blockContent = sb.ToString();
        await i.EvalAsync(blockContent);
    }

    [Primitive("THRU", HelpString = "THRU ( n1 n2 -- ) load and interpret blocks from n1 to n2")]
    private static async Task Prim_THRU(ForthInterpreter i)
    {
        i.EnsureStack(2, "THRU");
        var n2 = (int)ToLong(i.PopInternal());
        var n1 = (int)ToLong(i.PopInternal());
        for (int n = n1; n <= n2; n++)
        {
            i.EnsureBlockExistsOnDisk(n);
            var addr = i.GetOrAllocateBlockAddr(n);
            i.LoadBlockFromBacking(n, addr);
            i.SetCurrentBlockNumber(n);
            var sb = new StringBuilder(ForthInterpreter.BlockSize);
            for (int k = 0; k < ForthInterpreter.BlockSize; k++)
            {
                i.MemTryGet(addr + k, out var v);
                sb.Append((char)(v & 0xFF));
            }
            var blockContent = sb.ToString();
            await i.EvalAsync(blockContent);
        }
    }

    [Primitive("BUFFER", HelpString = "BUFFER ( u -- a-addr ) assign a block buffer to block u")]
    private static Task Prim_BUFFER(ForthInterpreter i)
    {
        i.EnsureStack(1, "BUFFER");
        var n = (int)ToLong(i.PopInternal());
        var addr = i.GetOrAllocateBlockAddr(n);
        i.Push((long)addr);
        return Task.CompletedTask;
    }

    [Primitive("EMPTY-BUFFERS", HelpString = "EMPTY-BUFFERS ( -- ) unassign all block buffers")]
    private static Task Prim_EMPTYBUFFERS(ForthInterpreter i)
    {
        i.ClearBlockBuffers();
        return Task.CompletedTask;
    }

    [Primitive("OPEN-BLOCK-FILE", HelpString = "OPEN-BLOCK-FILE <path> open/create block file backing")]
    private static Task Prim_OPENBLOCKFILE(ForthInterpreter i)
    {
        string path = (i.Stack.Count > 0 && i.Stack[^1] is string s) ? (string)i.PopInternal() : i.ReadNextTokenOrThrow("Expected path after OPEN-BLOCK-FILE");
        i.OpenBlockFile(path);
        return Task.CompletedTask;
    }

    [Primitive("OPEN-BLOCK-DIR", HelpString = "OPEN-BLOCK-DIR <path> open/create per-block dir backing")]
    private static Task Prim_OPENBLOCKDIR(ForthInterpreter i)
    {
        string path = (i.Stack.Count > 0 && i.Stack[^1] is string s) ? (string)i.PopInternal() : i.ReadNextTokenOrThrow("Expected path after OPEN-BLOCK-DIR");
        i.OpenBlockFile(path, perBlock: true);
        return Task.CompletedTask;
    }

    [Primitive("FLUSH-BLOCK-FILE", HelpString = "FLUSH-BLOCK-FILE ( -- ) flush block backing store")]
    private static Task Prim_FLUSHBLOCKFILE(ForthInterpreter i)
    {
        i.FlushBlockFile();
        return Task.CompletedTask;
    }

    [Primitive("CLOSE-BLOCK-FILE", HelpString = "CLOSE-BLOCK-FILE ( -- ) close block backing store")]
    private static Task Prim_CLOSEBLOCKFILE(ForthInterpreter i)
    {
        i.CloseBlockFile();
        return Task.CompletedTask;
    }

    [Primitive("SCR", HelpString = "SCR ( -- addr ) - variable containing the block number most recently listed")]
    private static Task Prim_SCR(ForthInterpreter i)
    {
        i.Push(i.ScrAddr);
        return Task.CompletedTask;
    }

    [Primitive("UPDATE", HelpString = "UPDATE ( -- ) - mark the current block as updated")]
    private static Task Prim_UPDATE(ForthInterpreter i)
    {
        i._dirtyBlocks.Add(i._currentBlock);
        return Task.CompletedTask;
    }

    [Primitive("SAVE-BUFFERS", HelpString = "SAVE-BUFFERS ( -- ) - transfer the contents of each updated block buffer to mass storage")]
    private static Task Prim_SAVEBUFFERS(ForthInterpreter i)
    {
        foreach (var n in i._dirtyBlocks.ToArray())
        {
            if (i._blockAddrMap.TryGetValue(n, out var addr))
            {
                i.SaveBlockToBacking(n, addr, ForthInterpreter.BlockSize);
            }
        }
        i._dirtyBlocks.Clear();
        return Task.CompletedTask;
    }

    [Primitive("FLUSH", HelpString = "FLUSH ( -- ) - perform SAVE-BUFFERS then unassign all block buffers")]
    private static Task Prim_FLUSH(ForthInterpreter i)
    {
        Prim_SAVEBUFFERS(i);
        Prim_EMPTYBUFFERS(i);
        return Task.CompletedTask;
    }
}