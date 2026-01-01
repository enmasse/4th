#!/usr/bin/env dotnet-script
#r "nuget: xunit, 2.4.2"
#r "D:/source/4th/src/Forth.Core/bin/Debug/net9.0/Forth.Core.dll"

using Forth.Core;
using Forth.Core.Interpreter;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Test IO that queues lines
class TestIO : IForthIO
{
    private readonly Queue<string> _lines = new();
    public void AddLine(string line) => _lines.Enqueue(line);
    public void Print(string text) => Console.WriteLine($"[OUT] {text}");
    public void PrintNumber(long number) => Console.WriteLine($"[NUM] {number}");
    public void NewLine() => Console.WriteLine();
    public string? ReadLine() => _lines.Count > 0 ? _lines.Dequeue() : null;
    public int ReadKey() => -1;
    public bool KeyAvailable() => false;
}

async Task Main()
{
    Console.WriteLine("=== REFILL Investigation ===\n");
    
    var io = new TestIO();
    io.AddLine("hello world");
    io.AddLine("second line");
    var forth = new ForthInterpreter(io);

    Console.WriteLine("Step 1: Call REFILL");
    await forth.EvalAsync("REFILL DROP");
    Console.WriteLine($"Stack after REFILL DROP: {forth.Stack.Count} items");
    Console.WriteLine();

    Console.WriteLine("Step 2: Check _refillSource field");
    var refillSource = typeof(ForthInterpreter)
        .GetField("_refillSource", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        ?.GetValue(forth) as string;
    Console.WriteLine($"_refillSource = '{refillSource}'");
    
    var currentSource = typeof(ForthInterpreter)
        .GetField("_currentSource", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        ?.GetValue(forth) as string;
    Console.WriteLine($"_currentSource = '{currentSource}'");
    Console.WriteLine();

    Console.WriteLine("Step 3: Call SOURCE in NEW EvalAsync");
    await forth.EvalAsync("SOURCE");
    Console.WriteLine($"Stack after SOURCE: {forth.Stack.Count} items");
    
    if (forth.Stack.Count >= 2)
    {
        var len = (long)forth.Stack[forth.Stack.Count - 1];
        var addr = (long)forth.Stack[forth.Stack.Count - 2];
        Console.WriteLine($"SOURCE returned: addr={addr}, len={len}");
        
        if (len > 0 && len < 1000)
        {
            var str = forth.ReadMemoryString(addr, len);
            Console.WriteLine($"String at address: '{str}'");
        }
    }
    Console.WriteLine();
    
    Console.WriteLine("Step 4: Check _refillSource again");
    refillSource = typeof(ForthInterpreter)
        .GetField("_refillSource", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        ?.GetValue(forth) as string;
    Console.WriteLine($"_refillSource = '{refillSource}'");
    
    currentSource = typeof(ForthInterpreter)
        .GetField("_currentSource", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        ?.GetValue(forth) as string;
    Console.WriteLine($"_currentSource = '{currentSource}'");
    Console.WriteLine();
    
    Console.WriteLine("=== Analysis ===");
    Console.WriteLine("The problem is that when we call EvalAsync(\"SOURCE\"),");
    Console.WriteLine("it sets _currentSource to \"SOURCE\", which overwrites");
    Console.WriteLine("the SOURCE information we want to query!");
    Console.WriteLine();
    Console.WriteLine("CurrentSource property returns: _refillSource ?? _currentSource");
    Console.WriteLine("So if _refillSource is set, SOURCE should see it.");
    Console.WriteLine("But AllocateSourceString is called with CurrentSource,");
    Console.WriteLine("which should work... Let's trace deeper!");
}

await Main();
