using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Forth.Tests.Compliance;

public class VariableOnSameLineTests
{
    private readonly ITestOutputHelper _output;

    public VariableOnSameLineTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Variable_CanBeUsedImmediatelyOnSameLine()
    {
        var forth = new ForthInterpreter();
        // This is the pattern from ttester.4th line 86
        // VARIABLE #ERRORS 0 #ERRORS !
        await forth.EvalAsync("VARIABLE MYVAR 0 MYVAR !");
        
        // Should complete without error
        Assert.Empty(forth.Stack);
    }

    [Fact]
    public async Task Variable_DefinedAndFetched()
    {
        var forth = new ForthInterpreter();
        await forth.EvalAsync("VARIABLE MYVAR");
        await forth.EvalAsync("0 MYVAR !");
        await forth.EvalAsync("MYVAR @");
        
        Assert.Single(forth.Stack);
        Assert.Equal(0L, (long)forth.Stack[0]);
    }

    [Fact]
    public async Task Variable_DefinedOnSeparateLines()
    {
        var forth = new ForthInterpreter();
        await forth.EvalAsync("VARIABLE MYVAR");
        await forth.EvalAsync("0");
        await forth.EvalAsync("MYVAR");
        await forth.EvalAsync("!");
        
        Assert.Empty(forth.Stack);
    }

    [Fact]
    public async Task Variable_WithBaseOnStack()
    {
        var forth = new ForthInterpreter();
        // Simulate the ttester.4th scenario:
        // BASE @ leaves something on stack
        await forth.EvalAsync("BASE @");
        _output.WriteLine($"After BASE @: stack depth = {forth.Stack.Count}");
        
        // DECIMAL doesn't affect stack
        await forth.EvalAsync("DECIMAL");
        _output.WriteLine($"After DECIMAL: stack depth = {forth.Stack.Count}");
        
        // Now try the VARIABLE pattern
        await forth.EvalAsync("VARIABLE MYVAR 0 MYVAR !");
        _output.WriteLine($"After VARIABLE MYVAR 0 MYVAR !: stack depth = {forth.Stack.Count}");
        
        // Stack should have just the base value left
        Assert.Single(forth.Stack);
    }

    [Fact]
    public async Task TtesterPattern_Exact()
    {
        var forth = new ForthInterpreter();
        // Exact lines from ttester.4th lines 80-86
        await forth.EvalAsync("BASE @");
        _output.WriteLine($"After 'BASE @': stack = [{string.Join(", ", forth.Stack)}]");
        
        await forth.EvalAsync("DECIMAL");
        _output.WriteLine($"After 'DECIMAL': stack = [{string.Join(", ", forth.Stack)}]");
        
        await forth.EvalAsync("VARIABLE ACTUAL-DEPTH");
        _output.WriteLine($"After 'VARIABLE ACTUAL-DEPTH': stack = [{string.Join(", ", forth.Stack)}]");
        
        await forth.EvalAsync("CREATE ACTUAL-RESULTS 32 CELLS ALLOT");
        _output.WriteLine($"After 'CREATE ACTUAL-RESULTS 32 CELLS ALLOT': stack = [{string.Join(", ", forth.Stack)}]");
        
        await forth.EvalAsync("VARIABLE START-DEPTH");
        _output.WriteLine($"After 'VARIABLE START-DEPTH': stack = [{string.Join(", ", forth.Stack)}]");
        
        await forth.EvalAsync("VARIABLE XCURSOR");
        _output.WriteLine($"After 'VARIABLE XCURSOR': stack = [{string.Join(", ", forth.Stack)}]");
        
        await forth.EvalAsync("VARIABLE ERROR-XT");
        _output.WriteLine($"After 'VARIABLE ERROR-XT': stack = [{string.Join(", ", forth.Stack)}]");
        
        await forth.EvalAsync("VARIABLE #ERRORS 0 #ERRORS !");
        _output.WriteLine($"After 'VARIABLE #ERRORS 0 #ERRORS !': stack = [{string.Join(", ", forth.Stack)}]");
    }

    [Fact]
    public async Task TtesterPattern_AsOneString()
    {
        var forth = new ForthInterpreter();
        //  Evaluate as one big string like file loading does
        var code = @"BASE @
DECIMAL
VARIABLE ACTUAL-DEPTH
CREATE ACTUAL-RESULTS 32 CELLS ALLOT
VARIABLE START-DEPTH
VARIABLE XCURSOR
VARIABLE ERROR-XT
VARIABLE #ERRORS 0 #ERRORS !";
        
        _output.WriteLine("Evaluating as one string...");
        await forth.EvalAsync(code);
        _output.WriteLine($"Final stack = [{string.Join(", ", forth.Stack)}]");
    }

    [Fact]
    public async Task LoadActualTtesterFile_Lines80to90()
    {
        var forth = new ForthInterpreter();
        var root = GetRepoRoot();
        var ttesterPath = System.IO.Path.Combine(root, "tests", "ttester.4th");
        
        if (!System.IO.File.Exists(ttesterPath))
        {
            _output.WriteLine($"ttester.4th not found at {ttesterPath}");
            return;
        }
        
        var allLines = await System.IO.File.ReadAllLinesAsync(ttesterPath);
        
        // Lines 80-90 in 1-indexed are indices 79-89 in 0-indexed
        // So we want Skip(79) Take(11) to get indices 79-89 inclusive
        var relevantLines = allLines.Skip(79).Take(11).ToArray();
        var code = string.Join("\n", relevantLines);
        
        _output.WriteLine($"Code to evaluate (lines {80}-{90}):");
        for (int i = 0; i < relevantLines.Length; i++)
        {
            _output.WriteLine($"Line {80 + i}: {relevantLines[i]}");
        }
        _output.WriteLine("");
        
        await forth.EvalAsync(code);
        _output.WriteLine($"Final stack = [{string.Join(", ", forth.Stack)}]");
    }

    [Fact]
    public async Task LoadEntireTtesterFile()
    {
        var forth = new ForthInterpreter();
        var root = GetRepoRoot();
        var ttesterPath = System.IO.Path.Combine(root, "tests", "ttester.4th");
        
        if (!System.IO.File.Exists(ttesterPath))
        {
            _output.WriteLine($"ttester.4th not found at {ttesterPath}");
            return;
        }
        
        var content = await System.IO.File.ReadAllTextAsync(ttesterPath);
        _output.WriteLine($"Loading entire file ({content.Length} chars)");
        
        try
        {
            await forth.EvalAsync(content);
            _output.WriteLine("Success!");
            _output.WriteLine($"Final stack = [{string.Join(", ", forth.Stack)}]");
        }
        catch (System.Exception ex)
        {
            _output.WriteLine($"FAILED: {ex.Message}");
            _output.WriteLine($"Stack at failure = [{string.Join(", ", forth.Stack)}]");
            throw;
        }
    }

    [Fact]
    public void TokenizeLine86()
    {
        var line = "VARIABLE #ERRORS 0 #ERRORS !";
        var tokens = Forth.Core.Interpreter.Tokenizer.Tokenize(line);
        
        _output.WriteLine($"Tokenizing: {line}");
        _output.WriteLine($"Tokens ({tokens.Count}):");
        for (int i = 0; i < tokens.Count; i++)
        {
            _output.WriteLine($"  [{i}] = '{tokens[i]}'");
        }
    }

    [Fact]
    public async Task LoadFirstPartOfTtesterFile()
    {
        var forth = new ForthInterpreter();
        var root = GetRepoRoot();
        var ttesterPath = System.IO.Path.Combine(root, "tests", "ttester.4th");
        
        if (!System.IO.File.Exists(ttesterPath))
        {
            _output.WriteLine($"ttester.4th not found at {ttesterPath}");
            return;
        }
        
        var allLines = await System.IO.File.ReadAllLinesAsync(ttesterPath);
        
        // Take only lines 1-86 (indices 0-85)
        var relevantLines = allLines.Take(86).ToArray();
        var code = string.Join("\n", relevantLines);
        
        _output.WriteLine($"Loading first 86 lines ({code.Length} chars)");
        _output.WriteLine($"Last 5 lines:");
        for (int i = Math.Max(0, relevantLines.Length - 5); i < relevantLines.Length; i++)
        {
            _output.WriteLine($"Line {i + 1}: {relevantLines[i]}");
        }
        
        try
        {
            await forth.EvalAsync(code);
            _output.WriteLine("Success!");
            _output.WriteLine($"Final stack = [{string.Join(", ", forth.Stack)}]");
        }
        catch (System.Exception ex)
        {
            _output.WriteLine($"FAILED: {ex.Message}");
            _output.WriteLine($"Stack at failure = [{string.Join(", ", forth.Stack)}]");
            throw;
        }
    }

    [Fact]
    public async Task LoadFirst87Lines()
    {
        var forth = new ForthInterpreter();
        var root = GetRepoRoot();
        var ttesterPath = System.IO.Path.Combine(root, "tests", "ttester.4th");
        
        if (!System.IO.File.Exists(ttesterPath))
        {
            _output.WriteLine($"ttester.4th not found at {ttesterPath}");
            return;
        }
        
        var allLines = await System.IO.File.ReadAllLinesAsync(ttesterPath);
        
        // Take only lines 1-87 (indices 0-86)
        var relevantLines = allLines.Take(87).ToArray();
        var code = string.Join("\n", relevantLines);
        
        _output.WriteLine($"Loading first 87 lines ({code.Length} chars)");
        _output.WriteLine($"Last 3 lines:");
        for (int i = Math.Max(0, relevantLines.Length - 3); i < relevantLines.Length; i++)
        {
            _output.WriteLine($"Line {i + 1}: {relevantLines[i]}");
        }
        
        try
        {
            await forth.EvalAsync(code);
            _output.WriteLine("Success!");
            _output.WriteLine($"Final stack = [{string.Join(", ", forth.Stack)}]");
        }
        catch (System.Exception ex)
        {
            _output.WriteLine($"FAILED: {ex.Message}");
            _output.WriteLine($"Stack at failure = [{string.Join(", ", forth.Stack)}]");
            throw;
        }
    }

    [Theory]
    [InlineData(120)]
    [InlineData(125)]
    [InlineData(130)]
    [InlineData(135)]
    [InlineData(140)]
    [InlineData(145)]
    [InlineData(150)]
    public async Task LoadFirstNLines(int n)
    {
        var forth = new ForthInterpreter();
        var root = GetRepoRoot();
        var ttesterPath = System.IO.Path.Combine(root, "tests", "ttester.4th");
        
        if (!System.IO.File.Exists(ttesterPath))
        {
            _output.WriteLine($"ttester.4th not found at {ttesterPath}");
            return;
        }
        
        var allLines = await System.IO.File.ReadAllLinesAsync(ttesterPath);
        
        var relevantLines = allLines.Take(Math.Min(n, allLines.Length)).ToArray();
        var code = string.Join("\n", relevantLines);
        
        _output.WriteLine($"Loading first {relevantLines.Length} lines ({code.Length} chars)");
        
        try
        {
            await forth.EvalAsync(code);
            _output.WriteLine("Success!");
            _output.WriteLine($"Final stack = [{string.Join(", ", forth.Stack)}]");
        }
        catch (System.Exception ex)
        {
            _output.WriteLine($"FAILED at or before line {relevantLines.Length}: {ex.Message}");
            _output.WriteLine($"Stack at failure = [{string.Join(", ", forth.Stack)}]");
            _output.WriteLine($"Last 5 lines:");
            for (int i = Math.Max(0, relevantLines.Length - 5); i < relevantLines.Length; i++)
            {
                _output.WriteLine($"Line {i + 1}: {relevantLines[i]}");
            }
            throw;
        }
    }

    private static string GetRepoRoot()
    {
        var dir = new System.IO.DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
        while (dir != null)
        {
            if (System.IO.Directory.GetFiles(dir.FullName, "*.sln").Any())
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        return System.IO.Directory.GetCurrentDirectory();
    }
}
