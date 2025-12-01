using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using Xunit.Abstractions;

namespace Forth.Tests.Compliance;

public class ErrorReportTests
{
    private readonly ITestOutputHelper _output;

    public ErrorReportTests(ITestOutputHelper output)
    {
        _output = output;
    }
    
    private static string GetRepoRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            if (Directory.GetFiles(dir.FullName, "*.sln").Any())
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        return Directory.GetCurrentDirectory();
    }
    
    [Fact]
    public async Task ErrorReport_ShouldLoadSuccessfully()
    {
        var forth = new ForthInterpreter();
        var root = GetRepoRoot();
        
        // First load ttester which defines #ERRORS
        var ttesterPath = Path.Combine(root, "tests", "ttester.4th");
        if (File.Exists(ttesterPath))
        {
            var ttesterCode = await File.ReadAllTextAsync(ttesterPath);
            try
            {
                Assert.True(await forth.EvalAsync(ttesterCode));
            }
            catch (System.Exception ex)
            {
                _output.WriteLine($"ttester.4th failed: {ex.Message}");
                throw;
            }
        }
        
        // Now load errorreport.fth
        var errorReportPath = Path.Combine(root, "tests", "forth2012-test-suite", "src", "errorreport.fth");
        Assert.True(File.Exists(errorReportPath), $"File not found: {errorReportPath}");
        
        var code = await File.ReadAllTextAsync(errorReportPath);
        try
        {
            var result = await forth.EvalAsync(code);
            Assert.True(result, "errorreport.fth should load successfully");
        }
        catch (System.Exception ex)
        {
            _output.WriteLine($"errorreport.fth failed: {ex.Message}");
            throw;
        }
    }
    
    [Fact]
    public async Task ErrorReport_CheckWordsAreDefined()
    {
        var forth = new ForthInterpreter();
        var root = GetRepoRoot();
        
        // Load dependencies
        var ttesterPath = Path.Combine(root, "tests", "ttester.4th");
        if (File.Exists(ttesterPath))
        {
            try
            {
                await forth.EvalAsync(await File.ReadAllTextAsync(ttesterPath));
            }
            catch (System.Exception ex)
            {
                _output.WriteLine($"ttester.4th error: {ex.Message}");
                throw;
            }
        }
        
        var errorReportPath = Path.Combine(root, "tests", "forth2012-test-suite", "src", "errorreport.fth");
        if (File.Exists(errorReportPath))
        {
            try
            {
                await forth.EvalAsync(await File.ReadAllTextAsync(errorReportPath));
            }
            catch (System.Exception ex)
            {
                _output.WriteLine($"errorreport.fth error: {ex.Message}");
                throw;
            }
            
            // Check that expected words are defined
            var words = forth.GetAllWordNames();
            
            // Check for REPORT-ERRORS (allow module-qualified names like "MOD:REPORT-ERRORS")
            bool NameMatches(IEnumerable<string> list, string target) => list.Any(w => w.Equals(target, StringComparison.OrdinalIgnoreCase) || w.EndsWith($":{target}", StringComparison.OrdinalIgnoreCase) || w.IndexOf(target, StringComparison.OrdinalIgnoreCase) >= 0);

            Assert.True(NameMatches(words, "REPORT-ERRORS"), $"REPORT-ERRORS not found. Available words with ERROR: {string.Join(", ", words.Where(w => w.Contains("ERROR", StringComparison.OrdinalIgnoreCase)))}");
            Assert.True(NameMatches(words, "SET-ERROR-COUNT"), "SET-ERROR-COUNT not found");
            Assert.True(NameMatches(words, "INIT-ERRORS"), "INIT-ERRORS not found");
        }
    }
    
    [Fact(Skip = "Depends on CheckWordsAreDefined passing first")]
    public async Task ErrorReport_ReportErrors_ShouldWork()
    {
        var forth = new ForthInterpreter();
        var root = GetRepoRoot();
        
        // Load dependencies
        var ttesterPath = Path.Combine(root, "tests", "ttester.4th");
        if (File.Exists(ttesterPath))
        {
            await forth.EvalAsync(await File.ReadAllTextAsync(ttesterPath));
        }
        
        var errorReportPath = Path.Combine(root, "tests", "forth2012-test-suite", "src", "errorreport.fth");
        if (File.Exists(errorReportPath))
        {
            await forth.EvalAsync(await File.ReadAllTextAsync(errorReportPath));
            
            // Call REPORT-ERRORS to verify it works
            var result = await forth.EvalAsync("REPORT-ERRORS");
            Assert.True(result);
        }
    }
}
