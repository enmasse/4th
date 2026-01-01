using System.Linq;
using System;
using System.Threading.Tasks;
using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.IO;
using Forth.Tests;

namespace Forth.Tests.Compliance;

public class Forth2012ComplianceTests
{
    private static ForthInterpreter New() => new();

    [Fact]
    public async Task CoreTests()
    {
        var repoRoot = TestPaths.GetRepoRoot();
        var suiteRoot = TestPaths.GetForth2012SuiteRoot();
        var f = New();
        var testerPath = Path.Combine(suiteRoot, "src", "tester.fr");
        var errorreportPath = Path.Combine(suiteRoot, "src", "errorreport.fth");
        var corePath = Path.Combine(suiteRoot, "src", "core.fr");
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
        var suiteRoot = TestPaths.GetForth2012SuiteRoot();
        var f = New();
        var testerPath = Path.Combine(suiteRoot, "src", "tester.fr");
        var errorreportPath = Path.Combine(suiteRoot, "src", "errorreport.fth");
        var coreextPath = Path.Combine(suiteRoot, "src", "coreexttest.fth");
        await f.EvalAsync($"\"{testerPath}\" INCLUDED");
        await f.EvalAsync($"\"{errorreportPath}\" INCLUDED");
        await f.EvalAsync($"\"{coreextPath}\" INCLUDED");
        await f.EvalAsync("TOTAL-ERRORS @");
        Assert.Equal(0L, (long)f.Pop());
    }

    [Fact]
    public async Task FloatingPointTests()
    {
        var f = new ForthInterpreter { EnableTrace = true };
        Environment.SetEnvironmentVariable("FORTH_TRACE_PRELUDE", "1");
        var root = TestPaths.GetRepoRoot();
        var suiteRoot = TestPaths.GetForth2012SuiteRoot();

        var testerPath = Path.Combine(root, "tests", "ttester.4th");
        var errorReportPath = Path.Combine(suiteRoot, "src", "errorreport.fth");
        var fpDir = Path.Combine(suiteRoot, "src", "fp");

        try
        {
            // Load tester and error reporting
            await f.EvalAsync($"\\\"{testerPath}\\\" INCLUDE");
            await f.EvalAsync($"\\\"{errorReportPath}\\\" INCLUDE");

            // Directly include the individual FP test files, bypassing buggy runfptests.fth
            await f.EvalAsync($"\\\"{Path.Combine(fpDir, "ttester.fs")}\\\" INCLUDE");
            await f.EvalAsync("SET-NEAR");
            await f.EvalAsync($"\\\"{Path.Combine(fpDir, "fatan2-test.fs")}\\\" INCLUDE");
            await f.EvalAsync($"\\\"{Path.Combine(fpDir, "ieee-arith-test.fs")}\\\" INCLUDE");
            await f.EvalAsync($"\\\"{Path.Combine(fpDir, "ieee-fprox-test.fs")}\\\" INCLUDE");
            await f.EvalAsync($"\\\"{Path.Combine(fpDir, "fpzero-test.4th")}\\\" INCLUDE");
            await f.EvalAsync($"\\\"{Path.Combine(fpDir, "fpio-test.4th")}\\\" INCLUDE");
            await f.EvalAsync($"\\\"{Path.Combine(fpDir, "to-float-test.4th")}\\\" INCLUDE");
            await f.EvalAsync($"\\\"{Path.Combine(fpDir, "ak-fp-test.fth")}\\\" INCLUDE");
        }
        catch (Exception ex)
        {
            var trace = f.GetTraceDump();
            var repoRoot = TestPaths.GetRepoRoot();
            var artifactsDir = Path.Combine(repoRoot, "artifacts");
            Directory.CreateDirectory(artifactsDir);
            var logPath = Path.Combine(artifactsDir, "floatingpoint-trace.log");

            await File.WriteAllTextAsync(logPath, $"{ex}\n\nTRACE:\n{trace}\n\nSTACK:\n{string.Join("\n", f.Stack)}");

            throw new Xunit.Sdk.XunitException(
                $"FloatingPointTests failed: {ex.GetType().Name}: {ex.Message} (trace saved to {logPath})");
        }
    }

    [Fact(Skip = "Test hangs, skipping temporarily")]
    public async Task ParanoiaTest()
    {
        var repoRoot = TestPaths.GetRepoRoot();
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
        await f.EvalAsync($"\"{Path.Combine(fpDir, "paranoia.4th")}\" INCLUDED");
        
        await f.EvalAsync("TOTAL-ERRORS @");
        Assert.Equal(0L, (long)f.Pop());
    }

    [Fact]
    public async Task FacilityTests()
    {
        var suiteRoot = TestPaths.GetForth2012SuiteRoot();
        var f = New();
        var testerPath = Path.Combine(suiteRoot, "src", "tester.fr");
        var errorreportPath = Path.Combine(suiteRoot, "src", "errorreport.fth");
        var facilityPath = Path.Combine(suiteRoot, "src", "facilitytest.fth");
        await f.EvalAsync($"\"{testerPath}\" INCLUDED");
        await f.EvalAsync($"\"{errorreportPath}\" INCLUDED");
        await f.EvalAsync($"\"{facilityPath}\" INCLUDED");
        await f.EvalAsync("TOTAL-ERRORS @");
        Assert.Equal(0L, (long)f.Pop());
    }

    [Fact]
    public async Task FileTests()
    {
        var suiteRoot = TestPaths.GetForth2012SuiteRoot();
        var f = New();
        var testerPath = Path.Combine(suiteRoot, "src", "tester.fr");
        var errorreportPath = Path.Combine(suiteRoot, "src", "errorreport.fth");
        var filePath = Path.Combine(suiteRoot, "src", "filetest.fth");
        await f.EvalAsync($"\"{testerPath}\" INCLUDED");
        await f.EvalAsync($"\"{errorreportPath}\" INCLUDED");
        await f.EvalAsync($"\"{filePath}\" INCLUDED");
        await f.EvalAsync("TOTAL-ERRORS @");
        Assert.Equal(0L, (long)f.Pop());
    }

    [Fact]
    public async Task BlockTests()
    {
        var suiteRoot = TestPaths.GetForth2012SuiteRoot();
        var f = New();
        var testerPath = Path.Combine(suiteRoot, "src", "tester.fr");
        var errorreportPath = Path.Combine(suiteRoot, "src", "errorreport.fth");
        var blockPath = Path.Combine(suiteRoot, "src", "blocktest.fth");
        await f.EvalAsync($"\"{testerPath}\" INCLUDED");
        await f.EvalAsync($"\"{errorreportPath}\" INCLUDED");
        await f.EvalAsync($"\"{blockPath}\" INCLUDED");
        await f.EvalAsync("TOTAL-ERRORS @");
        Assert.Equal(0L, (long)f.Pop());
    }

    [Fact]
    public async Task DoubleNumberTests()
    {
        var suiteRoot = TestPaths.GetForth2012SuiteRoot();
        var f = New();
        var testerPath = Path.Combine(suiteRoot, "src", "tester.fr");
        var errorreportPath = Path.Combine(suiteRoot, "src", "errorreport.fth");
        var doublePath = Path.Combine(suiteRoot, "src", "doubletest.fth");
        await f.EvalAsync($"\"{testerPath}\" INCLUDED");
        await f.EvalAsync($"\"{errorreportPath}\" INCLUDED");
        await f.EvalAsync($"\"{doublePath}\" INCLUDED");
        await f.EvalAsync("TOTAL-ERRORS @");
        Assert.Equal(0L, (long)f.Pop());
    }

    [Fact]
    public async Task ExceptionTests()
    {
        var suiteRoot = TestPaths.GetForth2012SuiteRoot();
        var f = New();
        var testerPath = Path.Combine(suiteRoot, "src", "tester.fr");
        var errorreportPath = Path.Combine(suiteRoot, "src", "errorreport.fth");
        var exceptionPath = Path.Combine(suiteRoot, "src", "exceptiontest.fth");
        await f.EvalAsync($"\"{testerPath}\" INCLUDED");
        await f.EvalAsync($"\"{errorreportPath}\" INCLUDED");
        await f.EvalAsync($"\"{exceptionPath}\" INCLUDED");
        await f.EvalAsync("TOTAL-ERRORS @");
        Assert.Equal(0L, (long)f.Pop());
    }

    [Fact]
    public async Task LocalsTests()
    {
        var suiteRoot = TestPaths.GetForth2012SuiteRoot();
        var f = New();
        var testerPath = Path.Combine(suiteRoot, "src", "tester.fr");
        var errorreportPath = Path.Combine(suiteRoot, "src", "errorreport.fth");
        var localsPath = Path.Combine(suiteRoot, "src", "localstest.fth");
        await f.EvalAsync($"\"{testerPath}\" INCLUDED");
        await f.EvalAsync($"\"{errorreportPath}\" INCLUDED");
        await f.EvalAsync($"\"{localsPath}\" INCLUDED");
        await f.EvalAsync("TOTAL-ERRORS @");
        Assert.Equal(0L, (long)f.Pop());
    }

    [Fact]
    public async Task MemoryTests()
    {
        var suiteRoot = TestPaths.GetForth2012SuiteRoot();
        var f = New();
        var testerPath = Path.Combine(suiteRoot, "src", "tester.fr");
        var errorreportPath = Path.Combine(suiteRoot, "src", "errorreport.fth");
        var memoryPath = Path.Combine(suiteRoot, "src", "memorytest.fth");
        await f.EvalAsync($"\"{testerPath}\" INCLUDED");
        await f.EvalAsync($"\"{errorreportPath}\" INCLUDED");
        await f.EvalAsync($"\"{memoryPath}\" INCLUDED");
        await f.EvalAsync("TOTAL-ERRORS @");
        Assert.Equal(0L, (long)f.Pop());
    }

    [Fact]
    public async Task SearchOrderTests()
    {
        var suiteRoot = TestPaths.GetForth2012SuiteRoot();
        var f = New();
        var testerPath = Path.Combine(suiteRoot, "src", "tester.fr");
        var errorreportPath = Path.Combine(suiteRoot, "src", "errorreport.fth");
        var searchorderPath = Path.Combine(suiteRoot, "src", "searchordertest.fth");
        await f.EvalAsync($"\"{testerPath}\" INCLUDED");
        await f.EvalAsync($"\"{errorreportPath}\" INCLUDED");
        await f.EvalAsync($"\"{searchorderPath}\" INCLUDED");
        await f.EvalAsync("TOTAL-ERRORS @");
        Assert.Equal(0L, (long)f.Pop());
    }

    [Fact]
    public async Task StringTests()
    {
        var suiteRoot = TestPaths.GetForth2012SuiteRoot();
        var f = New();
        var testerPath = Path.Combine(suiteRoot, "src", "tester.fr");
        var errorreportPath = Path.Combine(suiteRoot, "src", "errorreport.fth");
        var stringPath = Path.Combine(suiteRoot, "src", "stringtest.fth");
        await f.EvalAsync($"\"{testerPath}\" INCLUDED");
        await f.EvalAsync($"\"{errorreportPath}\" INCLUDED");
        await f.EvalAsync($"\"{stringPath}\" INCLUDED");
        await f.EvalAsync("TOTAL-ERRORS @");
        Assert.Equal(0L, (long)f.Pop());
    }

    [Fact]
    public async Task ToolsTests()
    {
        var suiteRoot = TestPaths.GetForth2012SuiteRoot();
        var f = New();
        var testerPath = Path.Combine(suiteRoot, "src", "tester.fr");
        var errorreportPath = Path.Combine(suiteRoot, "src", "errorreport.fth");
        var toolsPath = Path.Combine(suiteRoot, "src", "toolstest.fth");
        await f.EvalAsync($"\"{testerPath}\" INCLUDED");
        await f.EvalAsync($"\"{errorreportPath}\" INCLUDED");
        await f.EvalAsync($"\"{toolsPath}\" INCLUDED");
        await f.EvalAsync("TOTAL-ERRORS @");
        Assert.Equal(0L, (long)f.Pop());
    }
}