using Forth.Core.Interpreter;
using Xunit;
using Xunit.Abstractions;
using System.Threading.Tasks;

namespace Forth.Tests.Diagnostics;

public class BracketConditionalMultiLineDiagnosticTests
{
    private readonly ITestOutputHelper _output;

    public BracketConditionalMultiLineDiagnosticTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Diagnose_MultiLine_OuterFalse_SkipsNested()
    {
        var f = new ForthInterpreter();
        
        // This should skip the nested [IF] and push 42 after the outer [THEN]
        var src = string.Join('\n', new[]
        {
            "0 [IF]",
            "  -1 [IF] 999 [THEN]",
            "[THEN]",
            "42"
        });
        
        _output.WriteLine("Evaluating:");
        _output.WriteLine(src);
        _output.WriteLine("");
        
        try
        {
            Assert.True(await f.EvalAsync(src));
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Exception: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
        
        _output.WriteLine($"Stack count: {f.Stack.Count}");
        foreach (var item in f.Stack)
        {
            _output.WriteLine($"  Stack item: {item}");
        }
        
        // Also test if 42 alone works
        var f2 = new ForthInterpreter();
        await f2.EvalAsync("42");
        _output.WriteLine($"Control test - stack count: {f2.Stack.Count}, value: {f2.Stack[0]}");
        
        Assert.Single(f.Stack);
        Assert.Equal(42L, (long)f.Stack[0]);
    }
}
