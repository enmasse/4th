using System.Linq;
using System.Threading.Tasks;
using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.IO;

namespace Forth.Tests.Compliance;

public class Forth2012ComplianceTests
{
    private static ForthInterpreter New() => new();

    private static string GetRepoRoot()
    {
        // Search upward for the solution file to find the repo root
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            if (Directory.GetFiles(dir.FullName, "*.sln").Any())
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        // Fallback: assume we're in bin\Debug\net9.0
        return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", ".."));
    }

    [Fact]
    public async Task CoreTests()
    {
        var repoRoot = GetRepoRoot();
        var f = New();
        var testerPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "tester.fr");
        var errorreportPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "errorreport.fth");
        var corePath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "core.fr");
        await f.EvalAsync($"\"{testerPath}\" INCLUDED");
        await f.EvalAsync($"\"{errorreportPath}\" INCLUDED");
        await f.EvalAsync($"\"{corePath}\" INCLUDED");
        await f.EvalAsync("CORE-ERRORS SET-ERROR-COUNT");
        await f.EvalAsync("TOTAL-ERRORS @");
        Assert.Equal(0L, (long)f.Pop());
    }

    [Fact]
    public async Task CoreExtTests()
    {
        var repoRoot = GetRepoRoot();
        var f = New();
        var testerPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "tester.fr");
        var errorreportPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "errorreport.fth");
        var coreextPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "coreexttest.fth");
        await f.EvalAsync($"\"{testerPath}\" INCLUDED");
        await f.EvalAsync($"\"{errorreportPath}\" INCLUDED");
        await f.EvalAsync($"\"{coreextPath}\" INCLUDED");
        await f.EvalAsync("TOTAL-ERRORS @");
        Assert.Equal(0L, (long)f.Pop());
    }
}