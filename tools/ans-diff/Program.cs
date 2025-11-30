using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

class Program
{
    private static readonly IReadOnlyDictionary<string, string[]> WordSets =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["core"] = new[]
            {
                // Core (subset already tracked + a few common ones)
                ":", ";", "IMMEDIATE", "POSTPONE", "[", "]", "'" ,"LITERAL",
                "IF", "ELSE", "THEN", "BEGIN", "WHILE", "REPEAT", "UNTIL", "DO", "LOOP", "LEAVE", "UNLOOP", "I", "J", "RECURSE",
                "CREATE", "DOES>", "VARIABLE", "CONSTANT", "VALUE", "TO", "DEFER", "IS", "MARKER", "FORGET",
                "@", "!", "C@", "C!", ",", "ALLOT", "HERE", "PAD", "COUNT", "MOVE", "FILL", "ERASE",
                ".", ".S", "CR", "EMIT", "TYPE", "WORDS", "<#", "HOLD", "#", "#S", "SIGN", "#>",
                "READ-FILE", "WRITE-FILE", "APPEND-FILE", "FILE-EXISTS", "INCLUDE",
                "SPAWN", "FUTURE", "TASK", "JOIN", "AWAIT", "TASK?",
                "CATCH", "THROW", "ABORT", "EXIT", "BYE", "QUIT",
                "BASE", "HEX", "DECIMAL", ">NUMBER", "STATE",
                "GET-ORDER", "SET-ORDER", "WORDLIST", "DEFINITIONS", "FORTH",
                "KEY", "KEY?", "ACCEPT", "EXPECT", "SOURCE", ">IN", "WORD",
                "OPEN-FILE", "CLOSE-FILE", "FILE-SIZE", "REPOSITION-FILE",
                "BLOCK", "LOAD", "SAVE", "BLK",
                "D+", "D-", "M*", "*/MOD", "SM/REM", "FM/MOD",
                "0<", "0>", "1+", "1-", "ABS", "2*", "2/", "U<", "UM*", "UM/MOD",
                "2DROP", "NIP", "TUCK", "?DUP", "SP!", "SP@", "BL", "2!", "2@", "CELL+", "CELLS", "CHAR+", "CHARS", "ALIGN", "2R@", "C,",
                // Additional ANS core words to track (not all implemented yet)
                ".\"",        // ."
                "ABORT\"",    // ABORT"
                ">BODY",
                "M/MOD",
                "S\""         // S"
            },
            ["core-ext"] = new[]
            {
                // Common Core-Ext words (not exhaustive)
                ".[S]", "[ELSE]", "[IF]", "[THEN]",
                ">R", "R>", "R@", "2>R", "2R>", "/STRING",
                "ENVIRONMENT?", "?DO", "+LOOP", "UNUSED", "WITHIN",
                "-TRAILING", "COMPARE", "SEARCH", "SOURCE-ID", "REFILL",
                "S>D", ">NUMBER", ">IN", "SAVE-INPUT", "RESTORE-INPUT",
                "CASE", "OF", "ENDOF", "ENDCASE",
                "ALLOCATE", "FREE", "RESIZE",
                "DEFER@", "DEFER!",
            },
            ["block"] = new[]
            {
                // BLOCK word set (subset)
                "BLOCK", "BUFFER", "FLUSH", "UPDATE", "EMPTY-BUFFERS", "LIST", "LOAD", "SAVE-BUFFERS", "SCR",
            },
            ["file"] = new[]
            {
                // File word set (subset)
                "INCLUDE-FILE", "INCLUDED", "RENAME-FILE", "DELETE-FILE", "CREATE-FILE", "READ-LINE", "WRITE-LINE",
                "FILE-POSITION", "FILE-STATUS", "CLOSE-FILE", "OPEN-FILE", "READ-FILE", "WRITE-FILE", "REPOSITION-FILE", "FILE-SIZE",
            },
            ["float"] = new[]
            {
                // Floating point word set (subset)
                "F.", "F!", "F@", "F+", "F-", "F*", "F/", "F>S", "S>F", "FABS", "FNEGATE",
                "FLOOR", "FROUND", "FEXP", "FLOG", "FSIN", "FCOS", "FTAN", "FASIN", "FACOS", "FATAN2",
                "FSQRT", "FTRUNC", "F~", "F<", "F=", "F0<", "F0=", "FMAX", "FMIN", "FVARIABLE", "FCONSTANT", "FM/MOD",
            },
            ["double-number"] = new[]
            {
                "2ROT", "D2*", "D2/", "D<", "D=", "D>S", "DABS", "DMAX", "DMIN", "DNEGATE", "DU<",
            },
            ["facility"] = new[]
            {
                "AT-XY", "PAGE", "MS", "TIME&DATE",
            },
            ["local"] = new[]
            {
                "(LOCAL)", "LOCALS|",
            },
            ["memory-allocation"] = new[]
            {
                "ALLOCATE", "FREE", "RESIZE",
            },
            ["programming-tools"] = new[]
            {
                "AHEAD", "CS-PICK", "CS-ROLL", "DUMP", "FORGET", "SEE", "WORDS",
            },
            ["search-order"] = new[]
            {
                "DEFINITIONS", "FORTH-WORDLIST", "GET-CURRENT", "GET-ORDER", "SEARCH-WORDLIST", "SET-CURRENT", "SET-ORDER", "ALSO", "ONLY", "PREVIOUS",
            },
            ["string"] = new[]
            {
                "-TRAILING", "/STRING", "BLANK", "CMOVE", "CMOVE>", "COMPARE", "SEARCH", "SLITERAL",
            },
            ["tools"] = new[]
            {
                ".S", "?", "DUMP", "SEE", "WORDS", "FORGET", "MARKER",
            },
            ["exception"] = new[]
            {
                "ABORT", "ABORT\"", "CATCH", "THROW",
            },
            ["block-ext"] = new[]
            {
                "THRU",
            },
            ["double-number-ext"] = Array.Empty<string>(),
            ["exception-ext"] = Array.Empty<string>(),
            ["facility-ext"] = Array.Empty<string>(),
            ["file-ext"] = Array.Empty<string>(),
            ["float-ext"] = Array.Empty<string>(),
            ["local-ext"] = Array.Empty<string>(),
            ["memory-allocation-ext"] = Array.Empty<string>(),
            ["programming-tools-ext"] = Array.Empty<string>(),
            ["search-order-ext"] = Array.Empty<string>(),
            ["string-ext"] = Array.Empty<string>(),
            ["tools-ext"] = Array.Empty<string>()
        };

    private static (Dictionary<string, string[]> selectedSets, string[] selectedNames) BuildSelectedSets(string[] args)
    {
        var setArg = args.FirstOrDefault(a => a.StartsWith("--sets=", StringComparison.OrdinalIgnoreCase));
        string[] requested;
        if (setArg == null)
        {
            // Default to Core-only tracking for CI gating
            requested = new[] { "core" };
        }
        else
        {
            var value = setArg.Substring("--sets=".Length);
            if (string.Equals(value, "all", StringComparison.OrdinalIgnoreCase))
                requested = WordSets.Keys.ToArray();
            else
                requested = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        var selected = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var key in requested)
        {
            if (WordSets.TryGetValue(key, out var list))
            {
                selected[key] = list;
            }
        }
        return (selected, requested);
    }

    static int Main(string[] args)
    {
        static string ResolveRepoRoot(string start)
        {
            var dir = new DirectoryInfo(start);
            while (dir != null)
            {
                var hasSln = Directory.EnumerateFiles(dir.FullName, "*.sln", SearchOption.TopDirectoryOnly).Any();
                if (hasSln) return dir.FullName;
                dir = dir.Parent;
            }
            return start;
        }

        var repoRoot = ResolveRepoRoot(Directory.GetCurrentDirectory());
        var (selectedSets, selectedNames) = BuildSelectedSets(args);

        var sb = new StringBuilder();

        sb.AppendLine($"Report generated on {DateTime.Now:yyyy-MM-dd}");
        sb.AppendLine();

        Console.WriteLine($"Tracking sets: {string.Join(", ", selectedNames)} (total words: {selectedSets.Values.Sum(v => v.Length)})");
        Console.WriteLine($"Report generated on {DateTime.Now:yyyy-MM-dd}"); // Include date for reference

        var csFiles = Directory.EnumerateFiles(repoRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains(Path.Combine("tools", "ans-diff"), StringComparison.OrdinalIgnoreCase))
            .Where(p => !p.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            .Where(p => !p.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var primNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        // Match the string literal inside [Primitive("...")], handling escaped quotes and optional whitespace
        var primRegex = new Regex(@"\[\s*Primitive\(\s*""(?<name>(?:\\.|[^""])*)""", RegexOptions.Compiled);
        foreach (var f in csFiles)
        {
            var txt = File.ReadAllText(f);
            foreach (Match m in primRegex.Matches(txt))
            {
                var raw = m.Groups["name"].Value;
                var name = Regex.Unescape(raw);
                primNames.Add(name);
            }
        }

        sb.AppendLine($"Found {primNames.Count} primitives in code:\n");
        foreach (var p in primNames.OrderBy(x => x)) sb.AppendLine(p);

        var totalMissing = 0;
        foreach (var (setName, words) in selectedSets.OrderBy(kvp => kvp.Key))
        {
            var found = words.Where(w => primNames.Contains(w)).OrderBy(s => s).ToArray();
            var missing = words.Where(w => !primNames.Contains(w)).OrderBy(s => s).ToArray();
            totalMissing += missing.Length;

            sb.AppendLine($"\n## {setName} ({words.Length} words)");
            sb.AppendLine($"Present ({found.Length}): {string.Join(", ", found)}");
            sb.AppendLine($"Missing ({missing.Length}): {string.Join(", ", missing)}");
        }

        var extras = primNames.Where(p => !selectedSets.Values.SelectMany(v => v).Contains(p)).OrderBy(s => s).ToArray();
        sb.AppendLine($"\n## Other primitives in code not in tracked sets ({extras.Length})");
        sb.AppendLine(string.Join(", ", extras));

        sb.AppendLine($"\nTotal missing tracked words: {totalMissing}");

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

        var failArg = args.FirstOrDefault(a => a.StartsWith("--fail-on-missing=", StringComparison.OrdinalIgnoreCase));
        var failOnMissing = true; // default: fail on missing (Core-only by default)
        if (failArg != null)
        {
            var val = failArg.Substring("--fail-on-missing=".Length);
            if (bool.TryParse(val, out var b)) failOnMissing = b;
        }

        if (failOnMissing && totalMissing > 0)
        {
            Console.WriteLine($"\nFailing due to {totalMissing} missing tracked ANS words across sets: {string.Join(", ", selectedNames)}");
            foreach (var (setName, words) in selectedSets.OrderBy(kvp => kvp.Key))
            {
                var missing = words.Where(w => !primNames.Contains(w)).OrderBy(s => s).ToArray();
                if (missing.Length > 0)
                {
                    Console.WriteLine($"  {setName}: {string.Join(", ", missing)}");
                }
            }
            return 2;
        }

        return 0;
    }
}
