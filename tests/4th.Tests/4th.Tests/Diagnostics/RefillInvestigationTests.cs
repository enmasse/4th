using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using Xunit.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;

namespace Forth.Tests.Diagnostics;

public class RefillInvestigationTests
{
    private readonly ITestOutputHelper _output;

    public RefillInvestigationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private sealed class RefillTestIO : IForthIO
    {
        private readonly Queue<string> _lines = new();
        public void AddLine(string line) => _lines.Enqueue(line);
        public void Print(string text) { }
        public void PrintNumber(long number) { }
        public void NewLine() { }
        public string? ReadLine() => _lines.Count > 0 ? _lines.Dequeue() : null;
        public int ReadKey() => -1;
        public bool KeyAvailable() => false;
    }

    [Fact]
    public async Task Investigation_RefillAndSourceInteraction()
    {
        var io = new RefillTestIO();
        io.AddLine("hello world");
        var forth = new ForthInterpreter(io);

        _output.WriteLine("=== REFILL Investigation ===");
        _output.WriteLine("");

        // Step 1: Call REFILL
        _output.WriteLine("Step 1: Call REFILL DROP");
        await forth.EvalAsync("REFILL DROP");
        _output.WriteLine($"Stack after REFILL DROP: {forth.Stack.Count} items");
        
        // Check internal state using reflection
        var refillSourceField = typeof(ForthInterpreter)
            .GetField("_refillSource", BindingFlags.NonPublic | BindingFlags.Instance);
        var currentSourceField = typeof(ForthInterpreter)
            .GetField("_currentSource", BindingFlags.NonPublic | BindingFlags.Instance);
        
        var refillSource = refillSourceField?.GetValue(forth) as string;
        var currentSource = currentSourceField?.GetValue(forth) as string;
        
        _output.WriteLine($"_refillSource = '{refillSource}'");
        _output.WriteLine($"_currentSource = '{currentSource}'");
        _output.WriteLine("");

        // Step 2: Call SOURCE in a NEW EvalAsync
        _output.WriteLine("Step 2: Call SOURCE in NEW EvalAsync");
        await forth.EvalAsync("SOURCE");
        _output.WriteLine($"Stack after SOURCE: {forth.Stack.Count} items");
        
        // Check state again
        refillSource = refillSourceField?.GetValue(forth) as string;
        currentSource = currentSourceField?.GetValue(forth) as string;
        
        _output.WriteLine($"After SOURCE call:");
        _output.WriteLine($"_refillSource = '{refillSource}'");
        _output.WriteLine($"_currentSource = '{currentSource}'");
        _output.WriteLine("");
        
        if (forth.Stack.Count >= 2)
        {
            var len = (long)forth.Stack[forth.Stack.Count - 1];
            var addr = (long)forth.Stack[forth.Stack.Count - 2];
            _output.WriteLine($"SOURCE returned: addr={addr}, len={len}");
            
            if (len > 0 && len < 1000)
            {
                var str = forth.ReadMemoryString(addr, len);
                _output.WriteLine($"String at memory: '{str}'");
                _output.WriteLine($"String.Length: {str.Length}");
            }
        }
        
        _output.WriteLine("");
        _output.WriteLine("=== ROOT CAUSE ANALYSIS ===");
        _output.WriteLine("Problem: When EvalAsync(\"SOURCE\") is called,");
        _output.WriteLine("it sets _currentSource = \"SOURCE\", overwriting refill data.");
        _output.WriteLine("But CurrentSource property should return _refillSource ?? _currentSource");
        _output.WriteLine("So SOURCE should see the refilled line... unless something clears _refillSource!");
    }

    [Fact]
    public async Task Investigation_RefillSourcePersistence()
    {
        var io = new RefillTestIO();
        io.AddLine("hello world");
        var forth = new ForthInterpreter(io);

        _output.WriteLine("=== Checking _refillSource Persistence ===");
        _output.WriteLine("");

        var refillSourceField = typeof(ForthInterpreter)
            .GetField("_refillSource", BindingFlags.NonPublic | BindingFlags.Instance);

        // Call REFILL
        await forth.EvalAsync("REFILL DROP");
        var refillAfterRefill = refillSourceField?.GetValue(forth) as string;
        _output.WriteLine($"After REFILL DROP: _refillSource = '{refillAfterRefill}'");

        // Call a simple command
        await forth.EvalAsync("1 2 +");
        var refillAfterArith = refillSourceField?.GetValue(forth) as string;
        _output.WriteLine($"After '1 2 +': _refillSource = '{refillAfterArith}'");

        // Call SOURCE
        await forth.EvalAsync("SOURCE");
        var refillAfterSource = refillSourceField?.GetValue(forth) as string;
        _output.WriteLine($"After 'SOURCE': _refillSource = '{refillAfterSource}'");
        
        _output.WriteLine("");
        _output.WriteLine("If _refillSource is null after SOURCE, we found the bug!");
    }
}
