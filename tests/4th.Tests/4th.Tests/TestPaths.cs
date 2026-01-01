using System;
using System.IO;
using System.Linq;

namespace Forth.Tests;

internal static class TestPaths
{
    internal static string GetRepoRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            if (Directory.GetFiles(dir.FullName, "*.sln").Any())
                return dir.FullName;
            dir = dir.Parent;
        }
        return Directory.GetCurrentDirectory();
    }

    internal static string GetForth2012SuiteRoot()
    {
        var repoRoot = GetRepoRoot();

        var local = Path.Combine(repoRoot, "tests", "forth2012-test-suite-local");
        if (Directory.Exists(local))
            return local;

        var upstream = Path.Combine(repoRoot, "tests", "forth2012-test-suite");
        if (Directory.Exists(upstream))
            return upstream;

        throw new DirectoryNotFoundException(
            $"Could not locate Forth 2012 test suite directory. Expected '{local}' or '{upstream}'.");
    }
}
