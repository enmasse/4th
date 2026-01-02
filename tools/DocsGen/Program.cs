using System.Text;
using DocsGen;
using Forth.Core.Documentation;

static string Slug(string word)
{
    // Keep it stable and filesystem-friendly
    var sb = new StringBuilder(word.Length);
    foreach (var ch in word)
    {
        if (char.IsLetterOrDigit(ch)) sb.Append(char.ToLowerInvariant(ch));
        else sb.Append('_');
    }
    return sb.ToString();
}

static string EscapeMd(string s) => s.Replace("`", "``");

static string? TryExtractStackEffect(string help)
{
    // HelpString convention seems to be: "NAME ( ... ) - ..."
    var open = help.IndexOf('(');
    var close = help.IndexOf(')', open + 1);
    if (open < 0 || close < 0 || close <= open) return null;
    return help.Substring(open, close - open + 1).Trim();
}

static string? TryExtractShortDescription(string help)
{
    // HelpString convention seems to be: "NAME ( ... ) - short description"
    var dash = help.IndexOf(" - ", StringComparison.Ordinal);
    if (dash < 0) return null;
    var s = help[(dash + 3)..].Trim();
    return string.IsNullOrWhiteSpace(s) ? null : s;
}

static string BuildWordDoc(
    string name,
    string? module,
    string? help,
    string? category,
    bool immediate,
    bool isAsync,
    IReadOnlyList<TestSnippetExtractor.Snippet> examples,
    IReadOnlyCollection<string> seeAlso,
    Func<string, string> wordLink)
{
    help ??= string.Empty;

    var stackEffect = TryExtractStackEffect(help);
    var shortDesc = TryExtractShortDescription(help);

    var title = module is null ? name : $"{name} ({module})";

    var sb = new StringBuilder();
    sb.AppendLine($"# {EscapeMd(title)}");
    sb.AppendLine();

    sb.AppendLine("## NAME");
    sb.AppendLine();
    sb.Append("`" + EscapeMd(name) + "`");
    if (!string.IsNullOrWhiteSpace(shortDesc)) sb.Append(" — " + EscapeMd(shortDesc!));
    sb.AppendLine();
    sb.AppendLine();

    sb.AppendLine("## SYNOPSIS");
    sb.AppendLine();
    if (!string.IsNullOrWhiteSpace(stackEffect))
        sb.AppendLine($"`{EscapeMd(name)} {EscapeMd(stackEffect!)}`");
    else
        sb.AppendLine($"`{EscapeMd(name)}`");
    sb.AppendLine();

    sb.AppendLine("## DESCRIPTION");
    sb.AppendLine();
    if (!string.IsNullOrWhiteSpace(help))
        sb.AppendLine(EscapeMd(help));
    else
        sb.AppendLine("(No description available.)");
    sb.AppendLine();

    sb.AppendLine("## FLAGS");
    sb.AppendLine();
    sb.AppendLine($"- Module: `{EscapeMd(module ?? "(core)")}`");
    if (!string.IsNullOrWhiteSpace(category)) sb.AppendLine($"- Category: `{EscapeMd(category!)}`");
    sb.AppendLine($"- Immediate: `{immediate}`");
    sb.AppendLine($"- Async: `{isAsync}`");
    sb.AppendLine();

    sb.AppendLine("## EXAMPLES");
    sb.AppendLine();

    if (examples.Count == 0)
    {
        sb.AppendLine("```forth");
        sb.AppendLine($"\\ TODO: add example for {name}");
        sb.AppendLine("```");
        sb.AppendLine();
    }
    else
    {
        foreach (var ex in examples)
        {
            sb.AppendLine("```forth");
            sb.AppendLine(ex.Code);
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine($"Source: `{EscapeMd(ex.SourceFile)}`");
            sb.AppendLine();
        }
    }

    sb.AppendLine("## SEE ALSO");
    sb.AppendLine();

    if (seeAlso.Count == 0)
    {
        sb.AppendLine("- (none yet)");
    }
    else
    {
        foreach (var sa in seeAlso.OrderBy(s => s, StringComparer.OrdinalIgnoreCase).Take(12))
        {
            sb.AppendLine($"- [`{EscapeMd(sa)}`]({wordLink(sa)})");
        }
    }

    return sb.ToString();
}

static string FindRepoRoot(string startDir)
{
    var dir = new DirectoryInfo(startDir);
    while (dir is not null)
    {
        if (File.Exists(Path.Combine(dir.FullName, "Forth.sln"))) return dir.FullName;
        dir = dir.Parent;
    }

    throw new InvalidOperationException("Could not locate repo root (missing Forth.sln). Run DocsGen from within the repository.");
}

var repoRoot = FindRepoRoot(AppContext.BaseDirectory);
var docsRoot = Path.Combine(repoRoot, "docs", "words");
Directory.CreateDirectory(docsRoot);

var words = WordCatalog.GetAll();
var allWordNames = words.Select(w => w.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

var snippetsByWord = TestSnippetExtractor.ExtractFromForthTests(repoRoot, allWordNames);
var seeAlsoByWord = TestSnippetExtractor.ComputeSeeAlso(snippetsByWord, allWordNames);

string WordDocFileName(string word, string? mod)
{
    var slug = Slug((mod is null ? "" : mod + "_") + word);
    return slug + ".md";
}

string WordLink(string word)
{
    // Link by name only; if multiple modules share name, this will link to core version.
    // The index includes module disambiguation when present.
    return WordDocFileName(word, mod: null);
}

var wordEntries = new List<(string Name, string? Module, string Path)>();

foreach (var w in words)
{
    var name = w.Name;
    var module = w.Module;
    var rel = Path.Combine("docs", "words", WordDocFileName(name, module));
    var full = Path.Combine(repoRoot, rel);

    snippetsByWord.TryGetValue(name, out var ex);
    ex ??= [];

    seeAlsoByWord.TryGetValue(name, out var seeAlso);
    seeAlso ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    var content = BuildWordDoc(
        name: name,
        module: module,
        help: w.HelpString,
        category: w.Category,
        immediate: w.IsImmediate,
        isAsync: w.IsAsync,
        examples: ex,
        seeAlso: seeAlso,
        wordLink: WordLink);

    Directory.CreateDirectory(Path.GetDirectoryName(full)!);
    File.WriteAllText(full, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

    wordEntries.Add((Name: name, Module: module, Path: rel.Replace('\\', '/')));
}

// A–Z index
var byNamePath = Path.Combine(repoRoot, "docs", "words", "by-name.md");
var byName = new StringBuilder();
byName.AppendLine("# Word index (A–Z)");
byName.AppendLine();
byName.AppendLine("> Generated file. Do not edit by hand.");
byName.AppendLine();

foreach (var e in wordEntries.OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase))
{
    var display = e.Module is null ? e.Name : $"{e.Name} ({e.Module})";
    byName.AppendLine($"- [`{EscapeMd(display)}`]({Path.GetFileName(e.Path)})");
}

File.WriteAllText(byNamePath, byName.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

Console.WriteLine($"Generated {wordEntries.Count} word pages into docs/words/.");
