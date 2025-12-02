using Forth.Core;
using Forth.Core.Interpreter;
using Xunit;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace Forth.Tests.Core.MissingWords;

public class TtesterIncludeTests
{
    [Fact]
    public async Task Ttester_CanInclude_WithoutErrors()
    {
        var forth = new ForthInterpreter();

        string? FindTtester()
        {
            var assemblyDir = Path.GetDirectoryName(typeof(TtesterIncludeTests).Assembly.Location) ?? Directory.GetCurrentDirectory();
            var dir = new DirectoryInfo(assemblyDir);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "tests", "ttester.4th");
                if (File.Exists(candidate)) return candidate;
                if (Directory.EnumerateFiles(dir.FullName, "*.sln", SearchOption.TopDirectoryOnly).Any())
                {
                    candidate = Path.Combine(dir.FullName, "tests", "ttester.4th");
                    if (File.Exists(candidate)) return candidate;
                }
                dir = dir.Parent;
            }
            // Fallback: search upward from repo root (current dir)
            dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "tests", "ttester.4th");
                if (File.Exists(candidate)) return candidate;
                dir = dir.Parent;
            }
            return null;
        }

        var path = FindTtester();
        Assert.False(string.IsNullOrEmpty(path), "Could not locate tests/ttester.4th for include test");
        var contents = File.ReadAllText(path!);
        var result = await forth.EvalAsync(contents);
        Assert.True(result);
    }

    [Fact]
    public async Task Ttester_Variable_HashERRORS_Initializes()
    {
        var forth = new ForthInterpreter();
        string? FindTtester()
        {
            var assemblyDir = Path.GetDirectoryName(typeof(TtesterIncludeTests).Assembly.Location) ?? Directory.GetCurrentDirectory();
            var dir = new DirectoryInfo(assemblyDir);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "tests", "ttester.4th");
                if (File.Exists(candidate)) return candidate;
                if (Directory.EnumerateFiles(dir.FullName, "*.sln", SearchOption.TopDirectoryOnly).Any())
                {
                    candidate = Path.Combine(dir.FullName, "tests", "ttester.4th");
                    if (File.Exists(candidate)) return candidate;
                }
                dir = dir.Parent;
            }
            dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "tests", "ttester.4th");
                if (File.Exists(candidate)) return candidate;
                dir = dir.Parent;
            }
            return null;
        }

        var path = FindTtester();
        Assert.False(string.IsNullOrEmpty(path), "Could not locate tests/ttester.4th for include test");
        var contents = File.ReadAllText(path!);
        Assert.True(await forth.EvalAsync(contents));
        // Check #ERRORS exists and is 0
        Assert.True(await forth.EvalAsync("#ERRORS @"));
        Assert.Single(forth.Stack);
        Assert.Equal(0L, (long)forth.Stack[0]);
    }
}
