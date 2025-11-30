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

    [Fact]
    public async Task Diagnostic_SimpleCreateDoes()
    {
        var forth = new ForthInterpreter();
        
        _output.WriteLine("Test 1: Simple CREATE without DOES>");
        await forth.EvalAsync("CREATE SIMPLE-VAR");
        var words1 = forth.GetAllWordNames().ToList();
        var hasSimple = words1.Any(w => w.Equals("SIMPLE-VAR", StringComparison.OrdinalIgnoreCase));
        _output.WriteLine($"SIMPLE-VAR defined: {hasSimple}");
        Assert.True(hasSimple, "Simple CREATE should define a word");
        
        // Execute it
        await forth.EvalAsync("SIMPLE-VAR");
        _output.WriteLine($"Stack after SIMPLE-VAR: {forth.Stack.Count} items");
        Assert.Single(forth.Stack);
    }

    [Fact]
    public async Task Diagnostic_CreateInsideColon()
    {
        var forth = new ForthInterpreter();
        
        _output.WriteLine("Test 2: CREATE inside colon definition");
        await forth.EvalAsync(": MAKE-VAR CREATE ;");
        var words1 = forth.GetAllWordNames().ToList();
        var hasMakeVar = words1.Any(w => w.Equals("MAKE-VAR", StringComparison.OrdinalIgnoreCase));
        _output.WriteLine($"MAKE-VAR defined: {hasMakeVar}");
        Assert.True(hasMakeVar, "MAKE-VAR should be defined");
        
        _output.WriteLine("Executing: MAKE-VAR TEST-VAR");
        await forth.EvalAsync("MAKE-VAR TEST-VAR");
        var words2 = forth.GetAllWordNames().ToList();
        var hasTestVar = words2.Any(w => w.Equals("TEST-VAR", StringComparison.OrdinalIgnoreCase));
        _output.WriteLine($"TEST-VAR defined: {hasTestVar}");
        Assert.True(hasTestVar, "TEST-VAR should be created by MAKE-VAR");
    }

    [Fact]
    public async Task Diagnostic_CreateDupCommaInColon()
    {
        var forth = new ForthInterpreter();
        
        _output.WriteLine("Test 3: CREATE DUP , inside colon definition");
        await forth.EvalAsync(": MAKE-CONST CREATE DUP , ;");
        var words1 = forth.GetAllWordNames().ToList();
        var hasMakeConst = words1.Any(w => w.Equals("MAKE-CONST", StringComparison.OrdinalIgnoreCase));
        _output.WriteLine($"MAKE-CONST defined: {hasMakeConst}");
        Assert.True(hasMakeConst, "MAKE-CONST should be defined");
        
        _output.WriteLine("Executing: 42 MAKE-CONST TEST-CONST");
        await forth.EvalAsync("42 MAKE-CONST TEST-CONST");
        var words2 = forth.GetAllWordNames().ToList();
        var hasTestConst = words2.Any(w => w.Equals("TEST-CONST", StringComparison.OrdinalIgnoreCase));
        _output.WriteLine($"TEST-CONST defined: {hasTestConst}");
        _output.WriteLine($"Stack depth after MAKE-CONST: {forth.Stack.Count}");
        Assert.True(hasTestConst, "TEST-CONST should be created by MAKE-CONST");
    }

    [Fact]
    public async Task Diagnostic_SimpleDoesPattern()
    {
        var forth = new ForthInterpreter();
        
        _output.WriteLine("Test 4: Simple DOES> pattern");
        await forth.EvalAsync(": CONSTANT-MAKER CREATE , DOES> @ ;");
        var words1 = forth.GetAllWordNames().ToList();
        var hasMaker = words1.Any(w => w.Equals("CONSTANT-MAKER", StringComparison.OrdinalIgnoreCase));
        _output.WriteLine($"CONSTANT-MAKER defined: {hasMaker}");
        Assert.True(hasMaker, "CONSTANT-MAKER should be defined");
        
        _output.WriteLine("Executing: 99 CONSTANT-MAKER MY-CONST");
        await forth.EvalAsync("99 CONSTANT-MAKER MY-CONST");
        var words2 = forth.GetAllWordNames().ToList();
        var hasMyConst = words2.Any(w => w.Equals("MY-CONST", StringComparison.OrdinalIgnoreCase));
        _output.WriteLine($"MY-CONST defined: {hasMyConst}");
        
        if (hasMyConst)
        {
            _output.WriteLine("Executing: MY-CONST");
            await forth.EvalAsync("MY-CONST");
            _output.WriteLine($"Stack: {string.Join(", ", forth.Stack)}");
            Assert.Single(forth.Stack);
            Assert.Equal(99L, (long)forth.Stack[0]);
        }
        else
        {
            Assert.True(hasMyConst, "MY-CONST should be created by CONSTANT-MAKER");
        }
    }

    [Fact]
    public async Task Diagnostic_ErrorCountPattern()
    {
        var forth = new ForthInterpreter();
        
        _output.WriteLine("Test 5: ERROR-COUNT pattern from errorreport.fth");
        await forth.EvalAsync("DECIMAL");
        await forth.EvalAsync(": ERROR-COUNT CREATE DUP , CELL+ DOES> @ ;");
        var words1 = forth.GetAllWordNames().ToList();
        var hasErrorCount = words1.Any(w => w.Equals("ERROR-COUNT", StringComparison.OrdinalIgnoreCase));
        _output.WriteLine($"ERROR-COUNT defined: {hasErrorCount}");
        Assert.True(hasErrorCount, "ERROR-COUNT should be defined");
        
        _output.WriteLine("Executing: 0 ERROR-COUNT CORE-ERRORS");
        await forth.EvalAsync("0 ERROR-COUNT CORE-ERRORS");
        var words2 = forth.GetAllWordNames().ToList();
        var hasCoreErrors = words2.Any(w => w.Equals("CORE-ERRORS", StringComparison.OrdinalIgnoreCase));
        _output.WriteLine($"CORE-ERRORS defined: {hasCoreErrors}");
        _output.WriteLine($"Stack depth after ERROR-COUNT: {forth.Stack.Count}");
        if (forth.Stack.Count > 0)
        {
            _output.WriteLine($"Stack: {string.Join(", ", forth.Stack)}");
        }
        
        if (hasCoreErrors)
        {
            _output.WriteLine("Executing: CORE-ERRORS");
            await forth.EvalAsync("CORE-ERRORS");
            _output.WriteLine($"Stack after CORE-ERRORS: {string.Join(", ", forth.Stack)}");
        }
        
        Assert.True(hasCoreErrors, "CORE-ERRORS should be created by ERROR-COUNT");
    }

    [Fact]
    public async Task Diagnostic_MultipleErrorCounts()
    {
        var forth = new ForthInterpreter();
        
        _output.WriteLine("Test 6: Multiple ERROR-COUNT calls");
        await forth.EvalAsync("DECIMAL");
        await forth.EvalAsync(": ERROR-COUNT CREATE DUP , CELL+ DOES> @ ;");
        
        _output.WriteLine("Executing: 0 ERROR-COUNT ERR1 ERROR-COUNT ERR2");
        await forth.EvalAsync("0 ERROR-COUNT ERR1 ERROR-COUNT ERR2");
        
        var words = forth.GetAllWordNames().ToList();
        var hasErr1 = words.Any(w => w.Equals("ERR1", StringComparison.OrdinalIgnoreCase));
        var hasErr2 = words.Any(w => w.Equals("ERR2", StringComparison.OrdinalIgnoreCase));
        
        _output.WriteLine($"ERR1 defined: {hasErr1}");
        _output.WriteLine($"ERR2 defined: {hasErr2}");
        _output.WriteLine($"Stack depth: {forth.Stack.Count}");
        if (forth.Stack.Count > 0)
        {
            _output.WriteLine($"Stack: {string.Join(", ", forth.Stack)}");
        }
        
        Assert.True(hasErr1, "ERR1 should be created");
        Assert.True(hasErr2, "ERR2 should be created");
    }

    [Fact]
    public async Task Diagnostic_DoesCollectingState()
    {
        var forth = new ForthInterpreter();
        
        _output.WriteLine("Test 7: Check DOES> collecting state");
        _output.WriteLine("Define: : TEST-DOES CREATE DOES> ;");
        await forth.EvalAsync(": TEST-DOES CREATE DOES> ;");
        
        var words1 = forth.GetAllWordNames().ToList();
        var hasTestDoes = words1.Any(w => w.Equals("TEST-DOES", StringComparison.OrdinalIgnoreCase));
        _output.WriteLine($"TEST-DOES defined: {hasTestDoes}");
        
        // Check if _doesCollecting leaked
        _output.WriteLine("Execute: TEST-DOES FOO");
        try
        {
            await forth.EvalAsync("TEST-DOES FOO");
            var words2 = forth.GetAllWordNames().ToList();
            var hasFoo = words2.Any(w => w.Equals("FOO", StringComparison.OrdinalIgnoreCase));
            _output.WriteLine($"FOO defined: {hasFoo}");
            _output.WriteLine($"Stack depth: {forth.Stack.Count}");
        }
        catch (System.Exception ex)
        {
            _output.WriteLine($"ERROR: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public async Task Diagnostic_StepByStepErrorCount()
    {
        var forth = new ForthInterpreter();
        
        _output.WriteLine("Test 8: Step-by-step ERROR-COUNT definition and use");
        
        _output.WriteLine("Step 1: DECIMAL");
        await forth.EvalAsync("DECIMAL");
        
        _output.WriteLine("Step 2: Define ERROR-COUNT");
        await forth.EvalAsync(": ERROR-COUNT");
        _output.WriteLine($"  _isCompiling: {forth._isCompiling}");
        
        _output.WriteLine("Step 3: Add CREATE to definition");
        // This won't work as separate steps, but let's test the full definition
        
        _output.WriteLine("Full definition: : ERROR-COUNT CREATE DUP , CELL+ DOES> @ ;");
        await forth.EvalAsync(": ERROR-COUNT CREATE DUP , CELL+ DOES> @ ;");
        
        var words1 = forth.GetAllWordNames().ToList();
        var hasErrorCount = words1.Any(w => w.Equals("ERROR-COUNT", StringComparison.OrdinalIgnoreCase));
        _output.WriteLine($"ERROR-COUNT defined after full definition: {hasErrorCount}");
        
        _output.WriteLine("\nStep 4: Push 0 on stack");
        await forth.EvalAsync("0");
        _output.WriteLine($"Stack: {string.Join(", ", forth.Stack)}");
        
        _output.WriteLine("\nStep 5: Call ERROR-COUNT with name TEST-ERR");
        await forth.EvalAsync("ERROR-COUNT TEST-ERR");
        
        var words2 = forth.GetAllWordNames().ToList();
        var hasTestErr = words2.Any(w => w.Equals("TEST-ERR", StringComparison.OrdinalIgnoreCase));
        _output.WriteLine($"TEST-ERR defined: {hasTestErr}");
        _output.WriteLine($"Stack after ERROR-COUNT: {string.Join(", ", forth.Stack)}");
        
        if (hasTestErr)
        {
            _output.WriteLine("\nStep 6: Execute TEST-ERR");
            await forth.EvalAsync("TEST-ERR");
            _output.WriteLine($"Stack after TEST-ERR: {string.Join(", ", forth.Stack)}");
        }
    }
}
