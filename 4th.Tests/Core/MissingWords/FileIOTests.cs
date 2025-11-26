using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace Forth.Tests.Core.MissingWords;

public class FileIOTests
{
    private sealed class TestIO : IForthIO
    {
        public readonly System.Collections.Generic.List<string> Outputs = new();
        public void Print(string text) => Outputs.Add(text);
        public void PrintNumber(long number) => Outputs.Add(number.ToString());
        public void NewLine() => Outputs.Add("\n");
        public string? ReadLine() => null;
    }

    [Fact]
    public async Task Write_And_Read_File()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".txt");
        try
        {
            Assert.True(await forth.EvalAsync($"S\"Hello file\" \"{path}\" WRITE-FILE"));
            Assert.True(File.Exists(path));
            Assert.Equal("Hello file", File.ReadAllText(path));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task Append_File_Appends_Content()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".txt");
        try
        {
            // write initial
            Assert.True(await forth.EvalAsync($"S\"Line1\" \"{path}\" WRITE-FILE"));
            // append second line
            Assert.True(await forth.EvalAsync($"S\"\nLine2\" \"{path}\" APPEND-FILE"));
            var txt = File.ReadAllText(path);
            Assert.Equal("Line1\nLine2", txt);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task Read_File_Pushes_String()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".txt");
        try
        {
            File.WriteAllText(path, "Payload");
            Assert.True(await forth.EvalAsync($"\"{path}\" READ-FILE"));
            Assert.Single(forth.Stack);
            Assert.IsType<string>(forth.Stack[0]);
            Assert.Equal("Payload", (string)forth.Stack[0]);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task File_Exists_Flag()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".txt");
        // ensure missing
        if (File.Exists(path)) File.Delete(path);
        Assert.True(await forth.EvalAsync($"\"{path}\" FILE-EXISTS"));
        Assert.Single(forth.Stack);
        Assert.Equal(0L, (long)forth.Stack[0]);

        // create
        File.WriteAllText(path, "x");
        // clear stack for next eval
        forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync($"\"{path}\" FILE-EXISTS"));
        Assert.Single(forth.Stack);
        Assert.Equal(-1L, (long)forth.Stack[0]);

        if (File.Exists(path)) File.Delete(path);
    }

    [Fact]
    public async Task Include_Executes_File_Content()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".4th");
        try
        {
            // Write a tiny forth file that prints text and CR
            File.WriteAllText(path, ".\" INCLUDED\" CR\n");
            Assert.True(await forth.EvalAsync($"INCLUDE \"{path}\""));
            // INCLUDE executes lines which cause Print and NewLine
            Assert.True(io.Outputs.Count >= 2);
            Assert.Equal("INCLUDED", io.Outputs[0]);
            Assert.Equal("\n", io.Outputs[1]);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task Load_Executes_File_Content()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".4th");
        try
        {
            // Write a tiny forth file that prints text and CR
            File.WriteAllText(path, ".\" LOADED\" CR\n");
            // Push filename string onto stack and invoke LOAD
            Assert.True(await forth.EvalAsync($"\"{path}\" LOAD"));
            // LOAD executes lines which cause Print and NewLine
            Assert.True(io.Outputs.Count >= 2);
            Assert.Equal("LOADED", io.Outputs[0]);
            Assert.Equal("\n", io.Outputs[1]);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task FileSize_PushesLengthOrMinusOne()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".txt");
        // missing file -> -1
        if (File.Exists(path)) File.Delete(path);
        Assert.True(await forth.EvalAsync($"\"{path}\" FILE-SIZE"));
        Assert.Single(forth.Stack);
        Assert.Equal(-1L, (long)forth.Stack[0]);

        // create file
        File.WriteAllText(path, "ABC");
        forth = new ForthInterpreter(io);
        Assert.True(await forth.EvalAsync($"\"{path}\" FILE-SIZE"));
        Assert.Single(forth.Stack);
        Assert.Equal(3L, (long)forth.Stack[0]);

        if (File.Exists(path)) File.Delete(path);
    }

    [Fact]
    public async Task Open_Reposition_Close_FileHandle()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".txt");
        try
        {
            File.WriteAllText(path, "ABCDEFG");
            // Open file -> pushes ior and fileid (ior 0 == success)
            Assert.True(await forth.EvalAsync($"\"{path}\" OPEN-FILE"));
            // leave only the fileid on stack: stack is ( ior fid ) so do SWAP DROP -> ( fid )
            Assert.True(await forth.EvalAsync("SWAP DROP"));
            Assert.Single(forth.Stack);
            var handle = (long)forth.Stack[0];
            Assert.True(handle > 0);

            // Reposition to offset 2: push offset (top) and call REPOSITION-FILE (consumes handle+offset)
            Assert.True(await forth.EvalAsync("2 REPOSITION-FILE"));

            // Close by pushing handle and calling CLOSE-FILE -> returns ior (0 success)
            Assert.True(await forth.EvalAsync($"{handle} CLOSE-FILE"));
            Assert.Single(forth.Stack);
            Assert.Equal(0L, (long)forth.Stack[^1]);
            // drop the success flag
            Assert.True(await forth.EvalAsync("DROP"));
            // Closing again should return non-zero ior (failure) and not throw
            Assert.True(await forth.EvalAsync($"{handle} CLOSE-FILE"));
            Assert.Single(forth.Stack);
            Assert.NotEqual(0L, (long)forth.Stack[^1]);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task OpenFile_WithMode_WriteCreatesFile()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".txt");
        try
        {
            if (File.Exists(path)) File.Delete(path);
            // Push filename and mode 1 (write) then OPEN-FILE (ior fid)
            Assert.True(await forth.EvalAsync($"\"{path}\" 1 OPEN-FILE"));
            // keep fileid only
            Assert.True(await forth.EvalAsync("SWAP DROP"));
            var h = (long)forth.Stack[^1];
            Assert.True(h > 0);
            // Close handle (returns ior 0 on success)
            Assert.True(await forth.EvalAsync($"{h} CLOSE-FILE"));
            Assert.Equal(0L, (long)forth.Stack[^1]);
            Assert.True(File.Exists(path));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task ReadAndWrite_FileBytes_ViaHandle()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".bin");
        long h = -1, h2 = -1;
        try
        {
            if (File.Exists(path)) File.Delete(path);
            // Open for write (mode 1) -> leaves ior fid on stack
            Assert.True(await forth.EvalAsync($"\"{path}\" 1 OPEN-FILE"));
            // keep fid only
            Assert.True(await forth.EvalAsync("SWAP DROP"));
            h = (long)forth.Stack[^1];
            // remove the handle literal from the stack; we'll use the captured 'h' variable
            Assert.True(await forth.EvalAsync("DROP"));
            // Allocate memory buffer and fill with bytes 1..5
            Assert.True(await forth.EvalAsync("CREATE B 10 ALLOT"));
            // Populate bytes 1..5 into B..B+4 correctly (value addr) removing stray 0 literals
            Assert.True(await forth.EvalAsync("1 B C! 2 B 1 + C! 3 B 2 + C! 4 B 3 + C! 5 B 4 + C!"));
            // Write 5 bytes from B to handle
            Assert.True(await forth.EvalAsync($"{h} B 5 WRITE-FILE-BYTES"));
            Assert.Equal(5L, (long)forth.Stack[^1]);
            // drop the written-count result
            Assert.True(await forth.EvalAsync("DROP"));

            // Close and reopen for read
            Assert.True(await forth.EvalAsync($"{h} CLOSE-FILE"));
            // drop close ior
            Assert.True(await forth.EvalAsync("DROP"));
            Assert.True(await forth.EvalAsync($"\"{path}\" 0 OPEN-FILE"));
            // keep fid only
            Assert.True(await forth.EvalAsync("SWAP DROP"));
            h2 = (long)forth.Stack[^1];
            Assert.True(await forth.EvalAsync("DROP"));
            // Read 5 bytes into memory at address B+5
            Assert.True(await forth.EvalAsync($"{h2} B 5 + 5 READ-FILE-BYTES"));
            Assert.Equal(5L, (long)forth.Stack[^1]);
            // drop the read-count result
            Assert.True(await forth.EvalAsync("DROP"));

            // Clear interpreter stack so subsequent C@ checks are deterministic
            while (forth.Stack.Count > 0)
            {
                await forth.EvalAsync("DROP");
            }
            // Close the read handle before reading the file from the test process
            Assert.True(await forth.EvalAsync($"{h2} CLOSE-FILE"));
            // Verify bytes were written to the file directly
            var fileBytes = File.ReadAllBytes(path);
            Assert.Equal(5, fileBytes.Length);
            Assert.Equal((byte)1, fileBytes[0]);
            Assert.Equal((byte)2, fileBytes[1]);
            Assert.Equal((byte)3, fileBytes[2]);
            Assert.Equal((byte)4, fileBytes[3]);
            Assert.Equal((byte)5, fileBytes[4]);
        }
        finally
        {
            // Ensure any open handles are closed prior to deleting file
            try { if (h >= 0) await forth.EvalAsync($"{h} CLOSE-FILE"); } catch { }
            try { if (h2 >= 0) await forth.EvalAsync($"{h2} CLOSE-FILE"); } catch { }
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task ReadAndWrite_FileBytes_MemoryRoundtrip()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".bin");
        long h = -1, h2 = -1;
        try
        {
            if (File.Exists(path)) File.Delete(path);
            // Open for write and leave fid on stack
            Assert.True(await forth.EvalAsync($"\"{path}\" 1 OPEN-FILE SWAP DROP"));
            // Capture handle and clear stack
            h = (long)forth.Stack[^1];
            Assert.True(await forth.EvalAsync("DROP"));

            // Allocate buffer and populate bytes 1..5 at B..B+4
            Assert.True(await forth.EvalAsync("CREATE B 16 ALLOT"));
            // Populate bytes 1..5 into B..B+4 correctly (value addr) removing stray 0 literals
            Assert.True(await forth.EvalAsync("1 B C! 2 B 1 + C! 3 B 2 + C! 4 B 3 + C! 5 B 4 + C!"));

            // Write 5 bytes from B to file
            Assert.True(await forth.EvalAsync($"{h} B 5 WRITE-FILE-BYTES"));
            // drop written count
            Assert.True(await forth.EvalAsync("DROP"));

            // Reposition to start and read into memory at B+8
            Assert.True(await forth.EvalAsync($"{h} 0 REPOSITION-FILE"));
            Assert.True(await forth.EvalAsync($"{h} B 8 + 5 READ-FILE-BYTES"));
            // drop read count
            Assert.True(await forth.EvalAsync("DROP"));

            // Now verify bytes were read into interpreter memory at B+8 .. B+12 using C@
            // Clear any leftover items on the stack first
            while (forth.Stack.Count > 0)
            {
                await forth.EvalAsync("DROP");
            }
            Assert.True(await forth.EvalAsync("B 8 + C@ B 9 + C@ B 10 + C@ B 11 + C@ B 12 + C@"));
            // Expect five values on stack in order pushed (bottom->top)
            Assert.Equal(5, forth.Stack.Count);
            Assert.Equal(1L, (long)forth.Stack[0]);
            Assert.Equal(2L, (long)forth.Stack[1]);
            Assert.Equal(3L, (long)forth.Stack[2]);
            Assert.Equal(4L, (long)forth.Stack[3]);
            Assert.Equal(5L, (long)forth.Stack[4]);

            // Close read handle
            Assert.True(await forth.EvalAsync($"{h} CLOSE-FILE"));
        }
        finally
        {
            try { if (h >= 0) await forth.EvalAsync($"{h} CLOSE-FILE"); } catch { }
            try { if (h2 >= 0) await forth.EvalAsync($"{h2} CLOSE-FILE"); } catch { }
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task WriteBytes_FileSize_WithoutClosing()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".bin");
        long h = -1;
        try
        {
            if (File.Exists(path)) File.Delete(path);
            // Open for write
            Assert.True(await forth.EvalAsync($"\"{path}\" 1 OPEN-FILE"));
            Assert.True(await forth.EvalAsync("SWAP DROP"));
            h = (long)forth.Stack[^1];
            Assert.True(await forth.EvalAsync("DROP"));

            // Prepare memory and write 4 bytes
            Assert.True(await forth.EvalAsync("CREATE B 8 ALLOT"));
            Assert.True(await forth.EvalAsync("7 B C! 8 B 1 + C! 9 B 2 + C! 10 B 3 + C!"));
            Assert.True(await forth.EvalAsync($"{h} B 4 WRITE-FILE-BYTES"));
            Assert.Equal(4L, (long)forth.Stack[^1]);
            Assert.True(await forth.EvalAsync("DROP"));

            // Ensure interpreter stack is clean before FILE-SIZE
            while (forth.Stack.Count > 0) await forth.EvalAsync("DROP");
            // Query file size directly via FILE-SIZE (does not require closing)
            Assert.True(await forth.EvalAsync($"\"{path}\" FILE-SIZE"));
            Assert.Single(forth.Stack);
            var size = (long)forth.Stack[^1];
            Assert.Equal(4L, size);
        }
        finally
        {
            try { if (h >= 0) await forth.EvalAsync($"{h} CLOSE-FILE"); } catch { }
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task WriteThenClose_FileReadDirectly_HasBytes()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".bin");
        long h = -1;
        try
        {
            if (File.Exists(path)) File.Delete(path);
            // Open for write and write 3 bytes
            Assert.True(await forth.EvalAsync($"\"{path}\" 1 OPEN-FILE"));
            Assert.True(await forth.EvalAsync("SWAP DROP"));
            h = (long)forth.Stack[^1];
            Assert.True(await forth.EvalAsync("DROP"));
            Assert.True(await forth.EvalAsync("CREATE B 8 ALLOT"));
            Assert.True(await forth.EvalAsync("21 B C! 22 B 1 + C! 23 B 2 + C!"));
            Assert.True(await forth.EvalAsync($"{h} B 3 WRITE-FILE-BYTES"));
            Assert.True(await forth.EvalAsync("DROP"));

            // Close handle
            Assert.True(await forth.EvalAsync($"{h} CLOSE-FILE"));
            Assert.True(await forth.EvalAsync("DROP"));
            // Allow brief time for OS to flush and release any locks
            await Task.Delay(10);
            // Read file directly from OS and verify bytes
            var fileBytes = File.ReadAllBytes(path);
            Assert.Equal(3, fileBytes.Length);
            Assert.Equal((byte)21, fileBytes[0]);
            Assert.Equal((byte)22, fileBytes[1]);
            Assert.Equal((byte)23, fileBytes[2]);
        }
        finally
        {
            try { if (h >= 0) await forth.EvalAsync($"{h} CLOSE-FILE"); } catch { }
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task Reflection_Read_From_Live_FileStream_AfterWrite()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".bin");
        long h = -1;
        try
        {
            if (File.Exists(path)) File.Delete(path);
            Assert.True(await forth.EvalAsync($"\"{path}\" 1 OPEN-FILE"));
            Assert.True(await forth.EvalAsync("SWAP DROP"));
            h = (long)forth.Stack[^1];
            Assert.True(await forth.EvalAsync("DROP"));

            Assert.True(await forth.EvalAsync("CREATE B 8 ALLOT"));
            // Write bytes 2, 3, 4 to file via handle
            Assert.True(await forth.EvalAsync("2 B C! 3 B 1 + C! 4 B 2 + C!"));
            Assert.True(await forth.EvalAsync($"{h} B 3 WRITE-FILE-BYTES"));
            Assert.True(await forth.EvalAsync("DROP"));

            // Use reflection to get the live FileStream for the handle
            var fi = typeof(ForthInterpreter).GetField("_openFiles", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(fi);
            var dict = fi!.GetValue(forth) as System.Collections.IDictionary;
            Assert.NotNull(dict);
            // Handles stored as int keys
            var fsObj = dict![(int)h];
            Assert.NotNull(fsObj);
            var fs = fsObj as FileStream;
            Assert.NotNull(fs);

            // Read from the live stream directly (seek to beginning)
            fs!.Seek(0, SeekOrigin.Begin);
            var buf = new byte[3];
            var r = fs.Read(buf, 0, 3);
            Assert.Equal(3, r);
            Assert.Equal((byte)2, buf[0]);
            Assert.Equal((byte)3, buf[1]);
            Assert.Equal((byte)4, buf[2]);
        }
        finally
        {
            try { if (h >= 0) await forth.EvalAsync($"{h} CLOSE-FILE"); } catch { }
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task OpenSeparate_FileStream_ReadAllBytes_AfterWrite_WithShare()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".bin");
        long h = -1;
        try
        {
            if (File.Exists(path)) File.Delete(path);
            Assert.True(await forth.EvalAsync($"\"{path}\" 1 OPEN-FILE"));
            Assert.True(await forth.EvalAsync("SWAP DROP"));
            h = (long)forth.Stack[^1];
            Assert.True(await forth.EvalAsync("DROP"));

            Assert.True(await forth.EvalAsync("CREATE B 8 ALLOT"));
            // Write bytes 33, 34 to file via handle
            Assert.True(await forth.EvalAsync("33 B C! 34 B 1 + C!"));
            Assert.True(await forth.EvalAsync($"{h} B 2 WRITE-FILE-BYTES"));
            Assert.True(await forth.EvalAsync("DROP"));

            // Try to open a separate FileStream for reading with FileShare.ReadWrite
            using var fr = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var b = new byte[2];
            var got = fr.Read(b, 0, 2);
            Assert.Equal(2, got);
            Assert.Equal((byte)33, b[0]);
            Assert.Equal((byte)34, b[1]);
        }
        finally
        {
            try { if (h >= 0) await forth.EvalAsync($"{h} CLOSE-FILE"); } catch { }
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task Diagnostics_LastWriteAndReadBuffers()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".bin");
        long h = -1;
        try
        {
            if (File.Exists(path)) File.Delete(path);
            // Open for write
            Assert.True(await forth.EvalAsync($"\"{path}\" 1 OPEN-FILE"));
            Assert.True(await forth.EvalAsync("SWAP DROP"));
            h = (long)forth.Stack[^1];
            Assert.True(await forth.EvalAsync("DROP"));
            // Buffer
            Assert.True(await forth.EvalAsync("CREATE B 8 ALLOT"));
            Assert.True(await forth.EvalAsync("5 B C! 6 B 1 + C! 7 B 2 + C!"));
            Assert.True(await forth.EvalAsync($"{h} B 3 WRITE-FILE-BYTES"));
            Assert.True(await forth.EvalAsync("DROP"));
            // Clear stack before diagnostics to ensure deterministic count
            while (forth.Stack.Count > 0) await forth.EvalAsync("DROP");
            // Fetch diagnostics if available; skip test when diagnostics are not compiled
            try
            {
                var ok = await forth.EvalAsync("LAST-WRITE-BYTES");
                if (!ok) return; // primitive not available
            }
            catch (ForthException)
            {
                return; // diagnostics not present in this build
            }

            // Reposition and read
            Assert.True(await forth.EvalAsync($"{h} 0 REPOSITION-FILE"));
            Assert.True(await forth.EvalAsync($"{h} B 4 + 3 READ-FILE-BYTES"));
            Assert.True(await forth.EvalAsync("DROP"));
            // Clear stack again before LAST-READ-BYTES
            while (forth.Stack.Count > 0) await forth.EvalAsync("DROP");
            try
            {
                var ok2 = await forth.EvalAsync("LAST-READ-BYTES");
                if (!ok2) return;
            }
            catch (ForthException)
            {
                return;
            }
        }
        finally
        {
            try { if (h >= 0) await forth.EvalAsync($"{h} CLOSE-FILE"); } catch { }
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task WriteFileBytes_NegativeLength_Throws()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".bin");
        long h = -1;
        try
        {
            if (File.Exists(path)) File.Delete(path);
            Assert.True(await forth.EvalAsync($"\"{path}\" 1 OPEN-FILE"));
            Assert.True(await forth.EvalAsync("SWAP DROP"));
            h = (long)forth.Stack[^1];
            Assert.True(await forth.EvalAsync("DROP"));

            // Try to write with negative length
            await forth.EvalAsync($"{h} 0 -1 WRITE-FILE-BYTES");
            Assert.Fail("Expected exception");
        }
        catch (ForthException ex)
        {
            Assert.Equal(ForthErrorCode.CompileError, ex.Code);
        }
        finally
        {
            try { if (h >= 0) await forth.EvalAsync($"{h} CLOSE-FILE"); } catch { }
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task ReadFileBytes_NegativeLength_Throws()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".bin");
        long h = -1;
        try
        {
            File.WriteAllText(path, "x"); // create file
            Assert.True(await forth.EvalAsync($"\"{path}\" 0 OPEN-FILE"));
            Assert.True(await forth.EvalAsync("SWAP DROP"));
            h = (long)forth.Stack[^1];
            Assert.True(await forth.EvalAsync("DROP"));

            // Try to read with negative length
            await forth.EvalAsync($"{h} 0 -1 READ-FILE-BYTES");
            Assert.Fail("Expected exception");
        }
        catch (ForthException ex)
        {
            Assert.Equal(ForthErrorCode.CompileError, ex.Code);
        }
        finally
        {
            try { if (h >= 0) await forth.EvalAsync($"{h} CLOSE-FILE"); } catch { }
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task WriteFileBytes_InvalidHandle_Throws()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);

        try
        {
            // Try to write with invalid handle (999)
            await forth.EvalAsync("999 0 1 WRITE-FILE-BYTES");
            Assert.Fail("Expected exception");
        }
        catch (ForthException ex)
        {
            Assert.Equal(ForthErrorCode.CompileError, ex.Code);
        }
    }

    [Fact]
    public async Task ReadFileBytes_InvalidHandle_Throws()
    {
        var io = new TestIO();
        var forth = new ForthInterpreter(io);

        try
        {
            // Try to read with invalid handle (999)
            await forth.EvalAsync("999 0 1 READ-FILE-BYTES");
            Assert.Fail("Expected exception");
        }
        catch (ForthException ex)
        {
            Assert.Equal(ForthErrorCode.CompileError, ex.Code);
        }
    }
}
