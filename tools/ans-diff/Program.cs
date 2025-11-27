using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

class Program
{
    static readonly string[] AnsCore = new[]
    {
        // Core ANS Forth wordlist (subset/common core)
        ":", ";", "IMMEDIATE", "POSTPONE", "[", "]", "'", "LITERAL",
        "IF", "ELSE", "THEN", "BEGIN", "WHILE", "REPEAT", "UNTIL", "DO", "LOOP", "LEAVE", "UNLOOP", "I", "RECURSE",
        "CREATE", "DOES>", "VARIABLE", "CONSTANT", "VALUE", "TO", "DEFER", "IS", "MARKER", "FORGET",
        "@", "!", "C@", "C!", ",", "ALLOT", "HERE", "COUNT", "MOVE", "FILL", "ERASE",
        ".", ".S", "CR", "EMIT", "TYPE", "WORDS", "<#", "HOLD", "#", "#S", "SIGN", "#>",
        "READ-FILE", "WRITE-FILE", "APPEND-FILE", "FILE-EXISTS", "INCLUDE",
        "SPAWN", "FUTURE", "TASK", "JOIN", "AWAIT", "TASK?",
        "CATCH", "THROW", "ABORT", "EXIT", "BYE", "QUIT",
        "BASE", "HEX", "DECIMAL", ">NUMBER", "STATE",
        // Missing/advanced ones
        "GET-ORDER", "SET-ORDER", "WORDLIST", "DEFINITIONS", "FORTH",
        "KEY", "KEY?", "ACCEPT", "EXPECT", "SOURCE", ">IN",
        "OPEN-FILE", "CLOSE-FILE", "FILE-SIZE", "REPOSITION-FILE",
        "BLOCK", "LOAD", "SAVE", "BLK",
        "D+", "D-", "M*", "*/MOD"
    };

    static int Main(string[] args)
    {
        var repoRoot = Directory.GetCurrentDirectory();
        // If executed from tools/ans-diff dir, assume repo root is two levels up
        if (Path.GetFileName(repoRoot).Equals("ans-diff", StringComparison.OrdinalIgnoreCase))
            repoRoot = Path.GetFullPath(Path.Combine(repoRoot, "..", ".."));
        // If executed from 4th dir, assume repo root is one level up
        else if (Path.GetFileName(repoRoot).Equals("4th", StringComparison.OrdinalIgnoreCase))
            repoRoot = Path.GetFullPath(Path.Combine(repoRoot, ".."));

        var sb = new StringBuilder();

        var csFiles = Directory.EnumerateFiles(repoRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains(Path.Combine("tools", "ans-diff")))
            .ToArray();

        var primNames = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        var primRegex = new Regex(@"\[Primitive\(\s*""(?<name>[^""]+)""", RegexOptions.Compiled);
        foreach (var f in csFiles)
        {
            var txt = File.ReadAllText(f);
            foreach (Match m in primRegex.Matches(txt))
            {
                primNames.Add(m.Groups["name"].Value);
            }
        }

        sb.AppendLine($"Found {primNames.Count} primitives in code:\n");
        foreach (var p in primNames) sb.AppendLine(p);

        sb.AppendLine();
        var ansSet = new HashSet<string>(AnsCore, StringComparer.OrdinalIgnoreCase);
        var found = primNames.Where(p => ansSet.Contains(p)).OrderBy(s => s).ToArray();
        var missing = AnsCore.Where(a => !primNames.Contains(a, StringComparer.OrdinalIgnoreCase)).OrderBy(s => s).ToArray();
        var extras = primNames.Where(p => !ansSet.Contains(p)).OrderBy(s => s).ToArray();

        sb.AppendLine($"\nANS core words present ({found.Length}):");
        foreach (var fnd in found) sb.AppendLine(fnd);

        sb.AppendLine($"\nOther primitives in code not in ANS list ({extras.Length}):");
        foreach (var e in extras) sb.AppendLine(e);

        // Write report to tools/ans-diff/report.md so callers don't need to redirect stdout
        try
        {
            var reportDir = Path.Combine(repoRoot, "tools", "ans-diff");
            Directory.CreateDirectory(reportDir);
            var reportPath = Path.Combine(reportDir, "report.md");
            File.WriteAllText(reportPath, sb.ToString());
            Console.WriteLine($"Wrote report to: {reportPath}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to write report: {ex.Message}");
        }

        // Exit with non-zero when missing ANS core words to allow CI to fail on regressions
        if (missing.Length > 0)
        {
            Console.WriteLine($"\nFailing due to {missing.Length} missing ANS core words.");
            return 2;
        }

        return 0;
    }
}
