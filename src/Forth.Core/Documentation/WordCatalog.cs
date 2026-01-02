using Forth.Core.Execution;

namespace Forth.Core.Documentation;

/// <summary>
/// Exposes interpreter word metadata for tooling (for example, documentation generation).
/// </summary>
public static class WordCatalog
{
    /// <summary>
    /// Metadata describing a single word.
    /// </summary>
    public sealed record WordInfo(
        string Name,
        string? Module,
        string? HelpString,
        string? Category,
        bool IsImmediate,
        bool IsAsync)
    {
        /// <summary>The word name as invoked in Forth source.</summary>
        public string Name { get; init; } = Name;

        /// <summary>The module the word belongs to, or <see langword="null"/> for the core module.</summary>
        public string? Module { get; init; } = Module;

        /// <summary>Optional help text (typically includes stack effect and a short description).</summary>
        public string? HelpString { get; init; } = HelpString;

        /// <summary>Optional category label.</summary>
        public string? Category { get; init; } = Category;

        /// <summary>Whether the word is immediate.</summary>
        public bool IsImmediate { get; init; } = IsImmediate;

        /// <summary>Whether the word is asynchronous.</summary>
        public bool IsAsync { get; init; } = IsAsync;
    }

    /// <summary>
    /// Returns a stable, sorted list of exported words.
    /// </summary>
    public static IReadOnlyList<WordInfo> GetAll()
    {
        return CorePrimitives.Words.Values
            .Select(w => new WordInfo(
                Name: w.Name ?? string.Empty,
                Module: w.Module,
                HelpString: w.HelpString,
                Category: w.Category,
                IsImmediate: w.IsImmediate,
                IsAsync: false))
            .OrderBy(w => w.Module ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(w => w.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
