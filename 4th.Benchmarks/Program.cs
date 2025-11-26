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
    private ForthInterpreter _forth;

    [GlobalSetup]
    public void Setup()
    {
        _forth = new ForthInterpreter();
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
}
