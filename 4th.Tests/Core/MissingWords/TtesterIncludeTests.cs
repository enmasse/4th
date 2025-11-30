using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;

namespace Forth.Tests.Core.MissingWords;

public class TtesterIncludeTests
{
    [Fact]
    public async Task Ttester_CanInclude_WithoutErrors()
    {
        var forth = new ForthInterpreter();
        // Set working directory to workspace root
        var prev = System.IO.Directory.GetCurrentDirectory();
        try
        {
            var root = System.IO.Path.GetFullPath(System.IO.Path.Combine(prev, "..", "..", "..", ".."));
            System.IO.Directory.SetCurrentDirectory(root);
            // Try to include ttester.4th - this should work without stack underflow
            var result = await forth.EvalAsync("INCLUDE \"tests/ttester.4th\"");
            Assert.True(result);
        }
        finally
        {
            System.IO.Directory.SetCurrentDirectory(prev);
        }
    }

    [Fact]
    public async Task Ttester_Variable_HashERRORS_Initializes()
    {
        var forth = new ForthInterpreter();
        var prev = System.IO.Directory.GetCurrentDirectory();
        try
        {
            var root = System.IO.Path.GetFullPath(System.IO.Path.Combine(prev, "..", "..", "..", ".."));
            System.IO.Directory.SetCurrentDirectory(root);
            // Load ttester
            Assert.True(await forth.EvalAsync("INCLUDE \"tests/ttester.4th\""));
            // Check #ERRORS exists and is 0
            Assert.True(await forth.EvalAsync("#ERRORS @"));
            Assert.Single(forth.Stack);
            Assert.Equal(0L, (long)forth.Stack[0]);
        }
        finally
        {
            System.IO.Directory.SetCurrentDirectory(prev);
        }
    }
}
