using Xunit;
using Xunit.Abstractions;
using Forth.Core;
using Forth.Core.Interpreter;

namespace Forth.Tests.Compliance;

public class ParanoiaIsolationTests
{
    private readonly ITestOutputHelper _output;

    public ParanoiaIsolationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ParanoiaInitializationTest()
    {
        // Test that the patched initialization code works
        var interp = new ForthInterpreter();
        
        // Run the initialization test file
        var testCode = @"
            s"" [UNDEFINED]"" dup pad c! pad char+ swap cmove 
            pad find nip 0=
        ";
        
        interp.EvalAsync(testCode).Wait();
        
        // Should have result on stack (true if [UNDEFINED] is undefined before defining it)
        Assert.NotEmpty(interp.Stack);
        _output.WriteLine($"Initialization test passed. Stack top: {interp.Stack.Last()}");
    }

    [Fact]
    public void ParanoiaWithStackTrace()
    {
        // Run paranoia with detailed stack tracing to find where it fails
        var interp = new ForthInterpreter();
        
        try
        {
            // Try multiple possible paths
            var possiblePaths = new[]
            {
                Path.Combine("..", "..", "..", "..", "tests", "forth2012-test-suite-local", "src", "fp", "paranoia.4th"),
                Path.Combine("tests", "forth2012-test-suite-local", "src", "fp", "paranoia.4th"),
                Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "tests", "forth2012-test-suite-local", "src", "fp", "paranoia.4th"))
            };
            
            string? path = null;
            foreach (var p in possiblePaths)
            {
                if (File.Exists(p))
                {
                    path = p;
                    break;
                }
            }
            
            if (path == null)
            {
                _output.WriteLine($"Paranoia file not found. Tried:");
                foreach (var p in possiblePaths)
                {
                    _output.WriteLine($"  - {Path.GetFullPath(p)}");
                }
                return; // Skip if file doesn't exist
            }
            
            _output.WriteLine($"Found paranoia at: {Path.GetFullPath(path)}");

            interp.EvalAsync($"\"{path}\" INCLUDED").Wait();
            
            _output.WriteLine("Paranoia completed successfully!");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Paranoia failed with: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // Log the Forth stack state
            _output.WriteLine($"\nForth stack contents ({interp.Stack.Count} items):");
            for (int i = 0; i < Math.Min(10, interp.Stack.Count); i++)
            {
                var item = interp.Stack[interp.Stack.Count - 1 - i];
                _output.WriteLine($"  [{i}]: {item} ({item?.GetType().Name})");
            }
            
            // Get inner exception details
            var inner = ex.InnerException;
            while (inner != null)
            {
                _output.WriteLine($"\nInner exception: {inner.Message}");
                inner = inner.InnerException;
            }
            
            throw; // Re-throw to fail the test
        }
    }

    [Fact]
    public void FindAllCmoveCallsInParanoia()
    {
        // Analyze the paranoia.4th file to find all CMOVE calls
        var possiblePaths = new[]
        {
            Path.Combine("..", "..", "..", "..", "tests", "forth2012-test-suite-local", "src", "fp", "paranoia.4th"),
            Path.Combine("tests", "forth2012-test-suite-local", "src", "fp", "paranoia.4th"),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "tests", "forth2012-test-suite-local", "src", "fp", "paranoia.4th"))
        };
        
        string? path = null;
        foreach (var p in possiblePaths)
        {
            if (File.Exists(p))
            {
                path = p;
                break;
            }
        }
        
        if (path == null)
        {
            _output.WriteLine($"Paranoia file not found. Tried:");
            foreach (var p in possiblePaths)
            {
                _output.WriteLine($"  - {Path.GetFullPath(p)}");
            }
            return;
        }

        var content = File.ReadAllText(path);
        var lines = content.Split('\n');
        
        _output.WriteLine("Searching for CMOVE/MOVE calls in paranoia.4th:\n");
        
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var upperLine = line.ToUpperInvariant();
            
            if (upperLine.Contains("CMOVE") || upperLine.Contains("MOVE"))
            {
                _output.WriteLine($"Line {i + 1}: {line.Trim()}");
                
                // Show context (2 lines before and after)
                for (int j = Math.Max(0, i - 2); j <= Math.Min(lines.Length - 1, i + 2); j++)
                {
                    if (j == i) continue;
                    _output.WriteLine($"  {j + 1}: {lines[j].Trim()}");
                }
                _output.WriteLine("");
            }
        }
    }

    [Fact]
    public void TestParanoiaPatchedSections()
    {
        // Test the specific sections that were patched
        var interp = new ForthInterpreter();
        
        // Test 1: First patched section (line 278-279)
        var test1 = @"
            s"" [UNDEFINED]"" dup pad c! pad char+ swap cmove 
            pad find nip 0=
        ";
        
        interp.EvalAsync(test1).Wait();
        Assert.NotEmpty(interp.Stack);
        var result1 = interp.PopInternal();
        _output.WriteLine($"First patch test result: {result1}");
        
        // Test 2: Second patched section (line 284-285)  
        var test2 = @"
            s"" [DEFINED]"" dup pad c! pad char+ swap cmove 
            pad find nip 0=
        ";
        
        interp.EvalAsync(test2).Wait();
        Assert.NotEmpty(interp.Stack);
        var result2 = interp.PopInternal();
        _output.WriteLine($"Second patch test result: {result2}");
    }
}
