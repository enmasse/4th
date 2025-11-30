using Forth.Core.Interpreter;
using Xunit;
using Xunit.Abstractions;
using System.Threading.Tasks;

namespace Forth.Tests.Compliance;

public class ValueInitializationBugTest
{
    private readonly ITestOutputHelper _output;

    public ValueInitializationBugTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Value_ShouldConsumeStackAtDefinition()
    {
        var forth = new ForthInterpreter();
        
        _output.WriteLine("Test: VALUE should consume top of stack for initialization");
        
        // Push a value, then define VALUE
        await forth.EvalAsync("42 VALUE TEST-VAL");
        
        _output.WriteLine($"Stack after 'TRUE VALUE TEST-VAL': [{string.Join(", ", forth.Stack)}]");
        
        // Stack should be empty - VALUE consumed the 42
        Assert.Empty(forth.Stack);
        
        // TEST-VAL should return 42
        await forth.EvalAsync("TEST-VAL");
        Assert.Single(forth.Stack);
        Assert.Equal(42L, (long)forth.Stack[0]);
    }

    [Fact]
    public async Task Value_TtesterPattern()
    {
        var forth = new ForthInterpreter();
        
        _output.WriteLine("Test: ttester.4th pattern");
        
        // Simulate ttester.4th lines 80-110
        await forth.EvalAsync("BASE @");  // [10]
        _output.WriteLine($"After BASE @: [{string.Join(", ", forth.Stack)}]");
        
        await forth.EvalAsync("DECIMAL");  // [10]
        _output.WriteLine($"After DECIMAL: [{string.Join(", ", forth.Stack)}]");
        
        // Skip to line 110
        await forth.EvalAsync("TRUE VALUE EXACT?");  // Should consume TRUE, leave [10]
        _output.WriteLine($"After TRUE VALUE EXACT?: [{string.Join(", ", forth.Stack)}]");
        
        // Stack should still have [10]
        Assert.Single(forth.Stack);
        Assert.Equal(10L, (long)forth.Stack[0]);
    }
}
