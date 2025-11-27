using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Forth.Core;
using Forth.Core.Interpreter;

namespace Forth.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<ForthBenchmarks>();
    }
}

[MemoryDiagnoser]
public class ForthBenchmarks
{
    private ForthInterpreter? _forth;
    private string? _blockFilePath;

    [GlobalSetup]
    public async Task Setup()
    {
#pragma warning disable RS1035 // Do not do file IO in analyzers
        _forth = new ForthInterpreter();
        // Create a temporary block file
        _blockFilePath = Path.Combine(Path.GetTempPath(), "forth_blocks.tmp");
        // Create a dummy block file with some data
        using (var fs = File.Create(_blockFilePath))
        {
            fs.SetLength(1024 * 10); // 10 blocks
        }
        // Open it for blocks
        await _forth.EvalAsync($"\"{_blockFilePath}\" OPEN-BLOCK-FILE");
#pragma warning restore RS1035
    }

    [GlobalCleanup]
    public void Cleanup()
    {
#pragma warning disable RS1035 // Do not do file IO in analyzers
        if (_blockFilePath != null && File.Exists(_blockFilePath))
            File.Delete(_blockFilePath);
#pragma warning restore RS1035
    }

    [Benchmark]
    public void StackPushPop_Long()
    {
        for (int i = 0; i < 1000; i++)
        {
            _forth.Push((long)i);
            _forth.Pop();
        }
    }

    [Benchmark]
    public void StackPushPop_String()
    {
        for (int i = 0; i < 1000; i++)
        {
            _forth.Push(i.ToString());
            _forth.Pop();
        }
    }

    [Benchmark]
    public void ArithmeticOperations()
    {
        for (int i = 0; i < 1000; i++)
        {
            _forth.Push((long)i);
            _forth.Push(1L);
            _forth.Push((long)i + 1);
            _forth.Pop();
            _forth.Pop();
            _forth.Pop();
        }
    }

    [Benchmark]
    public async Task EvalSimpleArithmetic()
    {
        await _forth.EvalAsync("1 2 +");
    }

    [Benchmark]
    public async Task EvalLoop()
    {
        await _forth.EvalAsync(": test 0 1000 0 DO I + LOOP ; test");
    }

    [Benchmark]
    public async Task EvalTypeString()
    {
        await _forth.EvalAsync("\"Hello World\" TYPE");
    }

    [Benchmark]
    public async Task EvalTypeCountedString()
    {
        // First create a counted string
        await _forth.EvalAsync("S\" Hello World\" DROP"); // DROP to leave addr on stack
        await _forth.EvalAsync("TYPE");
    }

    [Benchmark]
    public async Task EvalBlockLoad()
    {
        // Assume block file is set up
        await _forth.EvalAsync("1 BLOCK DROP");
    }

    [Benchmark]
    public async Task EvalBlockSave()
    {
        // Save a block
        await _forth.EvalAsync("CREATE B 1024 ALLOT B 1024 1 SAVE");
    }

    [Benchmark]
    public async Task EvalPicturedNumeric()
    {
        // <#
        await _forth.EvalAsync("<# 12345 #S #>");
    }
}
