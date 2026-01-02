using System.Text.RegularExpressions;

namespace DocsGen;

internal static class TestSnippetExtractor
{
    public sealed record Snippet(string SourceFile, string Code);

    public static IReadOnlyDictionary<string, List<Snippet>> ExtractFromForthTests(string repoRoot, IEnumerable<string> wordNames)
    {
        var wordSet = new HashSet<string>(wordNames, StringComparer.OrdinalIgnoreCase);
        var results = new Dictionary<string, List<Snippet>>(StringComparer.OrdinalIgnoreCase);

        var testsDir = Path.Combine(repoRoot, "tests");
        if (!Directory.Exists(testsDir)) return results;

        static bool IsFixture(string p)
            => p.EndsWith(".4th", StringComparison.OrdinalIgnoreCase)
            || p.EndsWith(".fth", StringComparison.OrdinalIgnoreCase)
            || p.EndsWith(".fr", StringComparison.OrdinalIgnoreCase)
            || p.EndsWith(".fs", StringComparison.OrdinalIgnoreCase);

        var fixtureFiles = Directory.EnumerateFiles(testsDir, "*.*", SearchOption.AllDirectories)
            .Where(IsFixture)
            .Where(p => !p.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            .Where(p => !p.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        // Heuristic #1: capture `T{ ... }T` blocks (ttester harness) as examples.
        var ttesterBlock = new Regex(@"T\{(?<code>.*?)\}\s*T", RegexOptions.Singleline | RegexOptions.Compiled);

        // Heuristic #2: if a file has very few/no T{ }T blocks, capture the first few non-empty lines
        // after a `TESTING ...` header as a simple usage example.
        var testingHeader = new Regex(@"^\s*TESTING\b(?<rest>.*)$", RegexOptions.Multiline | RegexOptions.Compiled);

        foreach (var file in fixtureFiles)
        {
            var text = File.ReadAllText(file);
            var rel = Path.GetRelativePath(repoRoot, file).Replace('\\', '/');

            var tMatches = ttesterBlock.Matches(text);
            foreach (Match m in tMatches)
            {
                var code = Normalize(m.Groups["code"].Value);
                AddSnippetForWords(results, wordSet, rel, code);
            }

            // If there are no ttester blocks, fall back to TESTING-based examples.
            if (tMatches.Count == 0)
            {
                foreach (Match hdr in testingHeader.Matches(text))
                {
                    var start = hdr.Index + hdr.Length;
                    var snippet = GrabFollowingLines(text, start, maxLines: 3);
                    if (string.IsNullOrWhiteSpace(snippet)) continue;
                    AddSnippetForWords(results, wordSet, rel, Normalize(snippet));
                }
            }
        }

        return results;
    }

    private static void AddSnippetForWords(
        Dictionary<string, List<Snippet>> results,
        HashSet<string> wordSet,
        string sourceFile,
        string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return;

        var wordsUsed = ExtractWordTokens(code)
            .Where(w => wordSet.Contains(w))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var w in wordsUsed)
        {
            if (!results.TryGetValue(w, out var list))
            {
                list = new List<Snippet>();
                results[w] = list;
            }

            if (list.Count >= 3) continue;
            list.Add(new Snippet(SourceFile: sourceFile, Code: code));
        }
    }

    public static IReadOnlyDictionary<string, HashSet<string>> ComputeSeeAlso(
        IReadOnlyDictionary<string, List<Snippet>> snippetsByWord,
        IEnumerable<string> allWords)
    {
        var all = new HashSet<string>(allWords, StringComparer.OrdinalIgnoreCase);
        var map = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var (word, snippets) in snippetsByWord)
        {
            var related = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var snip in snippets)
            {
                foreach (var tok in ExtractWordTokens(snip.Code))
                {
                    if (all.Contains(tok) && !tok.Equals(word, StringComparison.OrdinalIgnoreCase))
                        related.Add(tok);
                }
            }

            map[word] = related;
        }

        return map;
    }

    private static IEnumerable<string> ExtractWordTokens(string forth)
    {
        var lines = forth.Replace("\r\n", "\n").Split('\n');
        foreach (var raw in lines)
        {
            var line = raw;
            var slash = line.IndexOf('\\');
            if (slash >= 0) line = line[..slash];

            while (true)
            {
                var open = line.IndexOf('(');
                if (open < 0) break;
                var close = line.IndexOf(')', open + 1);
                if (close < 0) break;
                line = line.Remove(open, close - open + 1);
            }

            foreach (var part in line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            {
                if (part is "->" or "T{" or "}T") continue;

                // Basic literal filtering
                if (long.TryParse(part, out _)) continue;
                if (double.TryParse(part, out _)) continue;
                if (part.EndsWith('.') && long.TryParse(part[..^1], out _)) continue; // doubles like 1.

                if (part.StartsWith("S\"", StringComparison.OrdinalIgnoreCase)) continue;
                if (part.StartsWith("\"", StringComparison.OrdinalIgnoreCase)) continue;

                yield return part.Trim();
            }
        }
    }

    private static string Normalize(string code)
    {
        var s = code.Replace("\r\n", "\n").Trim();
        var lines = s.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0);
        return string.Join("\n", lines);
    }

    private static string GrabFollowingLines(string text, int startIndex, int maxLines)
    {
        var after = text[startIndex..].Replace("\r\n", "\n");
        var lines = after.Split('\n');
        var kept = new List<string>(maxLines);
        foreach (var l in lines)
        {
            var t = l.Trim();
            if (t.Length == 0) continue;
            if (t.StartsWith("\\")) continue;
            // stop if we hit another TESTING header
            if (t.StartsWith("TESTING", StringComparison.OrdinalIgnoreCase)) break;
            kept.Add(l);
            if (kept.Count >= maxLines) break;
        }
        return string.Join("\n", kept);
    }
}
