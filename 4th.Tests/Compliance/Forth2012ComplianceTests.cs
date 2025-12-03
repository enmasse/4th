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

    [Fact]
    public async Task FloatingPointTests()
    {
        var repoRoot = GetRepoRoot();
        var f = New();
        var fpDir = Path.Combine(repoRoot, "tests", "forth2012-test-suite-local", "src", "fp");
        var testerPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite-local", "src", "tester.fr");
        var errorreportPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite-local", "src", "errorreport.fth");
        
        // Load tester and error reporting
        await f.EvalAsync($"\"{testerPath}\" INCLUDED");
        await f.EvalAsync($"\"{errorreportPath}\" INCLUDED");
        
        // Define [undefined] word that runfptests.fth tried to check for
        await f.EvalAsync(": [undefined] bl word find nip 0= ; immediate");
        
        // Directly include the individual FP test files, bypassing buggy runfptests.fth
        await f.EvalAsync($"\"{Path.Combine(fpDir, "ttester.fs")}\" INCLUDED");
        await f.EvalAsync("SET-NEAR");
        await f.EvalAsync($"\"{Path.Combine(fpDir, "fatan2-test.fs")}\" INCLUDED");
        await f.EvalAsync($"\"{Path.Combine(fpDir, "ieee-arith-test.fs")}\" INCLUDED");
        await f.EvalAsync($"\"{Path.Combine(fpDir, "ieee-fprox-test.fs")}\" INCLUDED");
        await f.EvalAsync($"\"{Path.Combine(fpDir, "fpzero-test.4th")}\" INCLUDED");
        await f.EvalAsync($"\"{Path.Combine(fpDir, "fpio-test.4th")}\" INCLUDED");
        await f.EvalAsync($"\"{Path.Combine(fpDir, "to-float-test.4th")}\" INCLUDED");
        // await f.EvalAsync($"\"{Path.Combine(fpDir, "paranoia.4th")}\" INCLUDED");
        await f.EvalAsync($"\"{Path.Combine(fpDir, "ak-fp-test.fth")}\" INCLUDED");
        
        await f.EvalAsync("TOTAL-ERRORS @");
        Assert.Equal(0L, (long)f.Pop());
    }

    [Fact]
    public async Task FacilityTests()
    {
        var repoRoot = GetRepoRoot();
        var f = New();
        var testerPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "tester.fr");
        var errorreportPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "errorreport.fth");
        var facilityPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "facilitytest.fth");
        await f.EvalAsync($"\"{testerPath}\" INCLUDED");
        await f.EvalAsync($"\"{errorreportPath}\" INCLUDED");
        await f.EvalAsync($"\"{facilityPath}\" INCLUDED");
        await f.EvalAsync("TOTAL-ERRORS @");
        Assert.Equal(0L, (long)f.Pop());
    }

    [Fact]
    public async Task FileTests()
    {
        var repoRoot = GetRepoRoot();
        var f = New();
        var testerPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "tester.fr");
        var errorreportPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "errorreport.fth");
        var filePath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "filetest.fth");
        await f.EvalAsync($"\"{testerPath}\" INCLUDED");
        await f.EvalAsync($"\"{errorreportPath}\" INCLUDED");
        await f.EvalAsync($"\"{filePath}\" INCLUDED");
        await f.EvalAsync("TOTAL-ERRORS @");
        Assert.Equal(0L, (long)f.Pop());
    }

    [Fact]
    public async Task BlockTests()
    {
        var repoRoot = GetRepoRoot();
        var f = New();
        var testerPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "tester.fr");
        var errorreportPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "errorreport.fth");
        var blockPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "blocktest.fth");
        await f.EvalAsync($"\"{testerPath}\" INCLUDED");
        await f.EvalAsync($"\"{errorreportPath}\" INCLUDED");
        await f.EvalAsync($"\"{blockPath}\" INCLUDED");
        await f.EvalAsync("TOTAL-ERRORS @");
        Assert.Equal(0L, (long)f.Pop());
    }

    [Fact]
    public async Task DoubleNumberTests()
    {
        var repoRoot = GetRepoRoot();
        var f = New();
        var testerPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "tester.fr");
        var errorreportPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "errorreport.fth");
        var doublePath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "doubletest.fth");
        await f.EvalAsync($"\"{testerPath}\" INCLUDED");
        await f.EvalAsync($"\"{errorreportPath}\" INCLUDED");
        await f.EvalAsync($"\"{doublePath}\" INCLUDED");
        await f.EvalAsync("TOTAL-ERRORS @");
        Assert.Equal(0L, (long)f.Pop());
    }

    [Fact]
    public async Task ExceptionTests()
    {
        var repoRoot = GetRepoRoot();
        var f = New();
        var testerPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "tester.fr");
        var errorreportPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "errorreport.fth");
        var exceptionPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "exceptiontest.fth");
        await f.EvalAsync($"\"{testerPath}\" INCLUDED");
        await f.EvalAsync($"\"{errorreportPath}\" INCLUDED");
        await f.EvalAsync($"\"{exceptionPath}\" INCLUDED");
        await f.EvalAsync("TOTAL-ERRORS @");
        Assert.Equal(0L, (long)f.Pop());
    }

    [Fact]
    public async Task LocalsTests()
    {
        var repoRoot = GetRepoRoot();
        var f = New();
        var testerPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "tester.fr");
        var errorreportPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "errorreport.fth");
        var localsPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "localstest.fth");
        await f.EvalAsync($"\"{testerPath}\" INCLUDED");
        await f.EvalAsync($"\"{errorreportPath}\" INCLUDED");
        await f.EvalAsync($"\"{localsPath}\" INCLUDED");
        await f.EvalAsync("TOTAL-ERRORS @");
        Assert.Equal(0L, (long)f.Pop());
    }

    [Fact]
    public async Task MemoryTests()
    {
        var repoRoot = GetRepoRoot();
        var f = New();
        var testerPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "tester.fr");
        var errorreportPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "errorreport.fth");
        var memoryPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "memorytest.fth");
        await f.EvalAsync($"\"{testerPath}\" INCLUDED");
        await f.EvalAsync($"\"{errorreportPath}\" INCLUDED");
        await f.EvalAsync($"\"{memoryPath}\" INCLUDED");
        await f.EvalAsync("TOTAL-ERRORS @");
        Assert.Equal(0L, (long)f.Pop());
    }

    [Fact]
    public async Task SearchOrderTests()
    {
        var repoRoot = GetRepoRoot();
        var f = New();
        var testerPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "tester.fr");
        var errorreportPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "errorreport.fth");
        var searchorderPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "searchordertest.fth");
        await f.EvalAsync($"\"{testerPath}\" INCLUDED");
        await f.EvalAsync($"\"{errorreportPath}\" INCLUDED");
        await f.EvalAsync($"\"{searchorderPath}\" INCLUDED");
        await f.EvalAsync("TOTAL-ERRORS @");
        Assert.Equal(0L, (long)f.Pop());
    }

    [Fact]
    public async Task StringTests()
    {
        var repoRoot = GetRepoRoot();
        var f = New();
        var testerPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "tester.fr");
        var errorreportPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "errorreport.fth");
        var stringPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "stringtest.fth");
        await f.EvalAsync($"\"{testerPath}\" INCLUDED");
        await f.EvalAsync($"\"{errorreportPath}\" INCLUDED");
        await f.EvalAsync($"\"{stringPath}\" INCLUDED");
        await f.EvalAsync("TOTAL-ERRORS @");
        Assert.Equal(0L, (long)f.Pop());
    }

    [Fact]
    public async Task ToolsTests()
    {
        var repoRoot = GetRepoRoot();
        var f = New();
        var testerPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "tester.fr");
        var errorreportPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "errorreport.fth");
        var toolsPath = Path.Combine(repoRoot, "tests", "forth2012-test-suite", "src", "toolstest.fth");
        await f.EvalAsync($"\"{testerPath}\" INCLUDED");
        await f.EvalAsync($"\"{errorreportPath}\" INCLUDED");
        await f.EvalAsync($"\"{toolsPath}\" INCLUDED");
        await f.EvalAsync("TOTAL-ERRORS @");
        Assert.Equal(0L, (long)f.Pop());
    }
}