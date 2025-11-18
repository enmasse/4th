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
}
