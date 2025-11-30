using Forth.Core.Interpreter;
using Xunit;
using Xunit.Abstractions;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace Forth.Tests.Compliance;

public class ErrorReportDiagnosticTests
{
    private readonly ITestOutputHelper _output;

    public ErrorReportDiagnosticTests(ITestOutputHelper output)
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
    public async Task Diagnostic_CheckWordDefinedAfterLoad()
    {
        var forth = new ForthInterpreter();
        var root = GetRepoRoot();

        // Load ttester first
        var ttesterPath = Path.Combine(root, "tests", "ttester.4th");
        if (File.Exists(ttesterPath))
        {
            _output.WriteLine("Loading ttester.4th...");
            await forth.EvalAsync(await File.ReadAllTextAsync(ttesterPath));
        }

        // Check words before loading errorreport
        var wordsBefore = forth.GetAllWordNames().ToList();
        _output.WriteLine($"Words before errorreport: {wordsBefore.Count}");
        _output.WriteLine($"Has #ERRORS: {wordsBefore.Any(w => w.Equals("#ERRORS", StringComparison.OrdinalIgnoreCase))}");

        // Load errorreport as a whole file
        var errorReportPath = Path.Combine(root, "tests", "forth2012-test-suite", "src", "errorreport.fth");
        var content = await File.ReadAllTextAsync(errorReportPath);
        
        _output.WriteLine($"\nLoading errorreport.fth ({content.Length} chars)...");
        var result = await forth.EvalAsync(content);
        _output.WriteLine($"EvalAsync returned: {result}");

        // Check words after loading
        var wordsAfter = forth.GetAllWordNames().ToList();
        _output.WriteLine($"\nWords after errorreport: {wordsAfter.Count}");
        _output.WriteLine($"New words: {wordsAfter.Count - wordsBefore.Count}");

        // List all ERROR-related words
        var errorWords = wordsAfter.Where(w => w.Contains("ERROR", StringComparison.OrdinalIgnoreCase)).ToList();
        _output.WriteLine($"\nERROR-related words ({errorWords.Count}):");
        foreach (var w in errorWords)
        {
            _output.WriteLine($"  - {w}");
        }

        // Check for specific words
        var checks = new[] { "REPORT-ERRORS", "SET-ERROR-COUNT", "INIT-ERRORS", "ERROR-COUNT", "CORE-ERRORS", "SHOW-ERROR-LINE", "HLINE" };
        foreach (var check in checks)
        {
            var found = wordsAfter.Any(w => w.Equals(check, StringComparison.OrdinalIgnoreCase));
            _output.WriteLine($"{check}: {(found ? "?" : "?")}");
        }

        // Check stack
        _output.WriteLine($"\nStack depth: {forth.Stack.Count}");
        if (forth.Stack.Count > 0)
        {
            _output.WriteLine($"Stack contents: {string.Join(", ", forth.Stack.Select(o => o?.ToString() ?? "null"))}");
        }
    }

    [Fact]
    public async Task Diagnostic_LoadErrorReportLineByLine()
    {
        var forth = new ForthInterpreter();
        var root = GetRepoRoot();

        // Load ttester first
        var ttesterPath = Path.Combine(root, "tests", "ttester.4th");
        if (File.Exists(ttesterPath))
        {
            _output.WriteLine("Loading ttester.4th...");
            await forth.EvalAsync(await File.ReadAllTextAsync(ttesterPath));
            _output.WriteLine("ttester.4th loaded successfully");
        }

        // Check if #ERRORS is defined
        var wordsAfterTtester = forth.GetAllWordNames().ToList();
        var hasErrors = wordsAfterTtester.Any(w => w.Equals("#ERRORS", StringComparison.OrdinalIgnoreCase));
        _output.WriteLine($"#ERRORS variable defined after ttester: {hasErrors}");

        // Now try loading errorreport.fth line by line
        var errorReportPath = Path.Combine(root, "tests", "forth2012-test-suite", "src", "errorreport.fth");
        var lines = await File.ReadAllLinesAsync(errorReportPath);

        _output.WriteLine($"\nProcessing {lines.Length} lines from errorreport.fth...");

        int lineNum = 0;
        foreach (var line in lines)
        {
            lineNum++;
            var trimmed = line.Trim();
            
            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("\\"))
            {
                continue;
            }

            _output.WriteLine($"Line {lineNum}: {trimmed}");

            try
            {
                await forth.EvalAsync(line);
                _output.WriteLine($"  ? OK");
            }
            catch (System.Exception ex)
            {
                _output.WriteLine($"  ? ERROR: {ex.Message}");
                
                // Stop on first error to diagnose
                var wordsNow = forth.GetAllWordNames().ToList();
                _output.WriteLine($"\nWords defined so far ({wordsNow.Count}):");
                foreach (var w in wordsNow.Where(w => w.Contains("ERROR", StringComparison.OrdinalIgnoreCase)))
                {
                    _output.WriteLine($"  - {w}");
                }
                
                throw;
            }
        }

        // Final check
        var finalWords = forth.GetAllWordNames().ToList();
        _output.WriteLine($"\n\nFinal word count: {finalWords.Count}");
        _output.WriteLine("ERROR-related words:");
        foreach (var w in finalWords.Where(w => w.Contains("ERROR", StringComparison.OrdinalIgnoreCase)))
        {
            _output.WriteLine($"  - {w}");
        }

        var hasReportErrors = finalWords.Any(w => w.Equals("REPORT-ERRORS", StringComparison.OrdinalIgnoreCase));
        Assert.True(hasReportErrors, "REPORT-ERRORS should be defined");
    }

    [Fact]
    public async Task Diagnostic_TestSimpleDefinition()
    {
        var forth = new ForthInterpreter();
        
        // Test if basic word definition works
        _output.WriteLine("Testing basic word definition...");
        await forth.EvalAsync(": TEST-WORD 42 ;");
        
        var words = forth.GetAllWordNames().ToList();
        var hasTestWord = words.Any(w => w.Equals("TEST-WORD", StringComparison.OrdinalIgnoreCase));
        _output.WriteLine($"TEST-WORD defined: {hasTestWord}");
        
        if (hasTestWord)
        {
            await forth.EvalAsync("TEST-WORD");
            Assert.Single(forth.Stack);
            Assert.Equal(42L, (long)forth.Stack[0]);
            _output.WriteLine("? TEST-WORD executed correctly, returned 42");
        }

        Assert.True(hasTestWord, "Basic word definition should work");
    }

    [Fact]
    public async Task Diagnostic_TestErrorCountPattern()
    {
        var forth = new ForthInterpreter();
        
        _output.WriteLine("Testing ERROR-COUNT pattern from errorreport.fth...");
        
        // Simulate the ERROR-COUNT pattern
        await forth.EvalAsync("DECIMAL");
        await forth.EvalAsync(": ERROR-COUNT CREATE DUP , CELL+ DOES> @ ;");
        await forth.EvalAsync("0 ERROR-COUNT TEST-ERRORS");
        
        var words = forth.GetAllWordNames().ToList();
        _output.WriteLine($"Words defined: {string.Join(", ", words.Where(w => w.Contains("ERROR", StringComparison.OrdinalIgnoreCase)))}");
        
        var hasTestErrors = words.Any(w => w.Equals("TEST-ERRORS", StringComparison.OrdinalIgnoreCase));
        _output.WriteLine($"TEST-ERRORS defined: {hasTestErrors}");
        
        if (hasTestErrors)
        {
            await forth.EvalAsync("TEST-ERRORS");
            _output.WriteLine($"Stack after TEST-ERRORS: {string.Join(", ", forth.Stack)}");
        }

        Assert.True(hasTestErrors, "ERROR-COUNT pattern should create words");
    }
}
