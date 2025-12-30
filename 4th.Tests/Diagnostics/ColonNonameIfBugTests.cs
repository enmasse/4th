using Xunit;
using Forth.Core;
using Forth.Core.Interpreter;
using System.Threading.Tasks;
using System.Linq;

namespace Forth.Tests.Diagnostics;

public class ColonNonameIfBugTests
{
    private static string GetRepoRoot()
    {
        var dir = new System.IO.DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
        while (dir != null)
        {
            if (System.IO.Directory.GetFiles(dir.FullName, "*.sln").Any())
                return dir.FullName;
            dir = dir.Parent;
        }
        return System.IO.Directory.GetCurrentDirectory();
    }

    [Fact]
    public async Task ColonNoname_InsideBracketIf_ShouldRestoreInterpretMode()
    {
        var f = new ForthInterpreter();
        
        // Define TESTING word inline (simplified version)
        await f.EvalAsync("VARIABLE VERBOSE");
        await f.EvalAsync("TRUE VERBOSE !");
        await f.EvalAsync(": TESTING SOURCE VERBOSE @ IF DUP >R TYPE CR R> >IN ! ELSE >IN ! DROP THEN ;");
        
        // The pattern from fatan2-test.fs lines 86-93
        var testCode = @"
VERBOSE @ [IF]
:noname  ( -- fp.separate? )
  depth >r 1e depth >r fdrop 2r> = ; execute
cr .( floating-point and data stacks )
[IF] .( *separate*) [ELSE] .( *not separate*) [THEN]
cr
[THEN]

testing normal values
";

        // This should throw "Undefined word: normal" (not "in definition")
        await f.EvalAsync(testCode);
    }

    [Fact]
    public async Task ColonNoname_InsideBracketIf_WithActualTtester_ShouldWork()
    {
        var f = new ForthInterpreter();
        
        // Use the exact same setup as FloatingPointTests
        var testerPath = System.IO.Path.Combine(GetRepoRoot(), "tests", "ttester.4th");
        await f.EvalAsync($"\"{testerPath}\" INCLUDED");
        
        // The pattern from fatan2-test.fs lines 86-93
        // verbose is FALSE by default in ttester, so the [IF] block should be SKIPPED
        var testCode = @"
verbose @ [IF]
:noname  ( -- fp.separate? )
  depth >r 1e depth >r fdrop 2r> = ; execute
cr .( floating-point and data stacks )
[IF] .( *separate*) [ELSE] .( *not separate*) [THEN]
cr
[THEN]

testing normal values
";

        // With verbose=FALSE, the [IF] block is skipped, so "testing normal values" executes
        // Since TESTING is an immediate word that skips the rest of the line,
        // "normal" and "values" should NOT be parsed
        await f.EvalAsync(testCode);  // Should NOT throw
    }
}
