using Forth.Core.Interpreter;
using System.IO;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive("WRITE-FILE", HelpString = "WRITE-FILE ( str filename -- ) Write string to file (overwrites)")]
    private static Task Prim_WRITEFILE(ForthInterpreter i)
    {
        i.EnsureStack(2, "WRITE-FILE");
        var fnameObj = i.PopInternal();
        var strObj = i.PopInternal();
        if (fnameObj is not string fname)
            throw new ForthException(ForthErrorCode.TypeError, "WRITE-FILE expects filename string");
        if (strObj is not string s)
            throw new ForthException(ForthErrorCode.TypeError, "WRITE-FILE expects string to write");

        File.WriteAllText(fname, s);
        return Task.CompletedTask;
    }

    [Primitive("APPEND-FILE", HelpString = "APPEND-FILE ( str filename -- ) Append string to file")]
    private static Task Prim_APPENDFILE(ForthInterpreter i)
    {
        i.EnsureStack(2, "APPEND-FILE");
        var fnameObj = i.PopInternal();
        var strObj = i.PopInternal();
        if (fnameObj is not string fname)
            throw new ForthException(ForthErrorCode.TypeError, "APPEND-FILE expects filename string");
        if (strObj is not string s)
            throw new ForthException(ForthErrorCode.TypeError, "APPEND-FILE expects string to append");

        File.AppendAllText(fname, s);
        return Task.CompletedTask;
    }

    [Primitive("READ-FILE", HelpString = "READ-FILE ( filename -- str ) Read entire file contents and push as string")]
    private static Task Prim_READFILE(ForthInterpreter i)
    {
        i.EnsureStack(1, "READ-FILE");
        var fnameObj = i.PopInternal();
        if (fnameObj is not string fname)
            throw new ForthException(ForthErrorCode.TypeError, "READ-FILE expects filename string");

        var text = File.ReadAllText(fname);
        i.Push(text);
        return Task.CompletedTask;
    }

    [Primitive("FILE-EXISTS", HelpString = "FILE-EXISTS ( filename -- flag ) Push -1 if exists, 0 if not")]
    private static Task Prim_FILEEXISTS(ForthInterpreter i)
    {
        i.EnsureStack(1, "FILE-EXISTS");
        var fnameObj = i.PopInternal();
        if (fnameObj is not string fname)
            throw new ForthException(ForthErrorCode.TypeError, "FILE-EXISTS expects filename string");

        i.Push(File.Exists(fname) ? -1L : 0L);
        return Task.CompletedTask;
    }

    [Primitive("FILE-SIZE", HelpString = "FILE-SIZE ( filename -- size ) Push file size in bytes or -1 on missing")]
    private static Task Prim_FILESIZE(ForthInterpreter i)
    {
        i.EnsureStack(1, "FILE-SIZE");
        var fnameObj = i.PopInternal();
        if (fnameObj is not string fname)
            throw new ForthException(ForthErrorCode.TypeError, "FILE-SIZE expects filename string");

        if (!File.Exists(fname))
        {
            i.Push(-1L);
            return Task.CompletedTask;
        }

        var fi = new FileInfo(fname);
        i.Push((long)fi.Length);
        return Task.CompletedTask;
    }

    [Primitive("INCLUDE", IsImmediate = true, IsAsync = true, HelpString = "INCLUDE <filename> - load and execute a Forth source file")]
    private static async Task Prim_INCLUDE(ForthInterpreter i)
    {
        var fname = i.ReadNextTokenOrThrow("Expected filename after INCLUDE");
        // Accept quoted filename token as produced by tokenizer
        if (fname.Length >= 2 && fname[0] == '"' && fname[^1] == '"')
        {
            fname = fname[1..^1];
        }

        if (!File.Exists(fname))
            throw new ForthException(ForthErrorCode.CompileError, $"INCLUDE: file not found: {fname}");

        var lines = await File.ReadAllLinesAsync(fname).ConfigureAwait(false);
        foreach (var raw in lines)
        {
            var line = raw?.Trim();
            if (string.IsNullOrEmpty(line)) continue;
            await i.EvalAsync(line).ConfigureAwait(false);
        }
    }

    // Runtime LOAD: takes a filename string on the stack and executes it (non-immediate)
    [Primitive("LOAD", IsAsync = true, HelpString = "LOAD ( filename -- ) - load and execute a Forth source file at runtime")]
    private static async Task Prim_LOAD(ForthInterpreter i)
    {
        i.EnsureStack(1, "LOAD");
        var fnameObj = i.PopInternal();
        if (fnameObj is not string fname)
            throw new ForthException(ForthErrorCode.TypeError, "LOAD expects filename string on stack");

        if (!File.Exists(fname))
            throw new ForthException(ForthErrorCode.CompileError, $"LOAD: file not found: {fname}");

        var lines = await File.ReadAllLinesAsync(fname).ConfigureAwait(false);
        foreach (var raw in lines)
        {
            var line = raw?.Trim();
            if (string.IsNullOrEmpty(line)) continue;
            await i.EvalAsync(line).ConfigureAwait(false);
        }
    }
}
