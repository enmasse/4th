using Forth.Core.Interpreter;
using Xunit;
using Xunit.Abstractions;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using Forth.Tests;

namespace Forth.Tests.Compliance;

public class ErrorReportTests
{
    private readonly ITestOutputHelper _output;

    public ErrorReportTests(ITestOutputHelper output)
    {
        _output = output;
    }
    
    private static string GetRepoRoot() => TestPaths.GetRepoRoot();
    
    [Fact]
    public async Task ErrorReport_ShouldLoadSuccessfully()
    {
        var forth = new ForthInterpreter();
        var root = GetRepoRoot();

        // Load ttester first
        var ttesterPath = Path.Combine(root, "tests", "ttester.4th");
        if (File.Exists(ttesterPath))
        {
            await forth.EvalAsync(await File.ReadAllTextAsync(ttesterPath));
        }

        var suiteRoot = TestPaths.GetForth2012SuiteRoot();
        var errorReportPath = Path.Combine(suiteRoot, "src", "errorreport.fth");
        if (!File.Exists(errorReportPath))
        {
            throw new FileNotFoundException($"File not found: {errorReportPath}");
        }

        var content = await File.ReadAllTextAsync(errorReportPath);
        var result = await forth.EvalAsync(content);
        Assert.True(result);
    }
}
