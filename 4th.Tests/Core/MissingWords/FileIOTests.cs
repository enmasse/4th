using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

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
            Assert.True(await forth.EvalAsync("1 0 B + C! 2 0 B 1 + C! 3 0 B 2 + C! 4 0 B 3 + C! 5 0 B 4 + C!"));
            // Write 5 bytes from B to handle
            Assert.True(await forth.EvalAsync($"{h} B 5 WRITE-FILE-BYTES"));
            Assert.Equal(5L, (long)forth.Stack[^1]);

            // Close and reopen for read
            Assert.True(await forth.EvalAsync($"{h} CLOSE-FILE"));
            Assert.True(await forth.EvalAsync($"\"{path}\" 0 OPEN-FILE"));
            // keep fid only
            Assert.True(await forth.EvalAsync("SWAP DROP"));
            h2 = (long)forth.Stack[^1];
            Assert.True(await forth.EvalAsync("DROP"));
            // Read 5 bytes into memory at address B+5
            Assert.True(await forth.EvalAsync($"{h2} B 5 + 5 READ-FILE-BYTES"));
            Assert.Equal(5L, (long)forth.Stack[^1]);

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
}
