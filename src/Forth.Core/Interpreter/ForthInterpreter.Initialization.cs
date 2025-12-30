using System.Threading.Tasks;

namespace Forth.Core.Interpreter;

// Partial: initialization, constructor, prelude loading
public partial class ForthInterpreter
{
    private readonly Task _loadPrelude;
    private bool _preludeLoaded = false;

    /// <summary>
    /// Create a new interpreter instance.
    /// </summary>
    /// <param name="io">Optional IO implementation to use for input/output. If null, a console IO is used.</param>
    /// <param name="blockCacheSize">Optional LRU block cache size (defaults to 64 if not provided).</param>
    public ForthInterpreter(IForthIO? io = null, int? blockCacheSize = null)
    {
        _io = io ?? new ConsoleForthIO();

        _stateAddr = _nextAddr++;
        _mem[_stateAddr] = 0;

        _baseAddr = _nextAddr++;
        _mem[_baseAddr] = 10;

        // Allocate memory cells for SOURCE and >IN variables
        _sourceAddr = _nextAddr++;
        _mem[_sourceAddr] = 0; // not storing string; reserved
        _inAddr = _nextAddr++;
        _mem[_inAddr] = 0; // >IN initial value

        _scrAddr = _nextAddr++;
        _mem[_scrAddr] = 0; // SCR initial value

        // Allocate stable PAD buffer address (ANS Forth compliant)
        // PAD should return a stable address above the dictionary space
        // Place it at a high address (900000) well below heap (1000000)
        _padAddr = 900000L;

        // No per-module lazy dictionaries anymore; use tuple-keyed _dict directly
        _baselineCount = _definitions.Count; // record baseline for core/compiler words
        _loadPrelude = LoadPreludeAsync(); // Load pure Forth definitions

        // Configure LRU cache size
        _maxCachedBlocks = blockCacheSize ?? 64;

        // Ensure basic block helper words are present in the dictionary (workaround for generator edge cases)
        if (!_dict.ContainsKey((null, "BLK")))
        {
            var blkWord = new Word(ii => { ii.Push((long)ii.GetCurrentBlockNumber()); return Task.CompletedTask; }) { Name = "BLK", Module = null };
            _dict = _dict.SetItem((null, "BLK"), blkWord);
        }

        // Add ENV wordlist words
        AddEnvWords();

        // Ensure REPORT-ERRORS exists as a stub so compliance helper files that
        // expect this word can be loaded and override it if necessary.
        if (!_dict.ContainsKey((null, "REPORT-ERRORS")))
        {
            var rep = new Word(ii => { return Task.CompletedTask; }) { Name = "REPORT-ERRORS", Module = null };
            _dict = _dict.SetItem((null, "REPORT-ERRORS"), rep);
        }
    }

    private async Task LoadPreludeAsync()
    {
        try
        {
            // Try to load embedded prelude resource
            var asm = typeof(ForthInterpreter).Assembly;
            var resourceName = "Forth.Core.prelude.4th";
            Trace($"Prelude load: trying embedded resource {resourceName}");
            using var stream = asm.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using var reader = new System.IO.StreamReader(stream);
                var prelude = await reader.ReadToEndAsync();
                await LoadPreludeText(prelude);
                _preludeLoaded = true;
                Trace("Prelude load: embedded resource loaded");
                return;
            }

            // Fallback: try loading from file system (for development)
            var preludePath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(asm.Location) ?? ".",
                "prelude.4th");

            if (System.IO.File.Exists(preludePath))
            {
                var prelude = await System.IO.File.ReadAllTextAsync(preludePath);
                await LoadPreludeText(prelude);
                _preludeLoaded = true;
                Trace($"Prelude load: file loaded from {preludePath}");
            }
            else
            {
                // No prelude available - mark as loaded to avoid blocking
                _preludeLoaded = true;
                Trace($"Prelude load: not found at {preludePath}");
            }
        }
        catch (System.Exception ex)
        {
            // If prelude fails to load, mark as loaded anyway to avoid blocking
            // The interpreter will still work without prelude words
            _preludeLoaded = true;
            // Store the exception for debugging
            System.Diagnostics.Debug.WriteLine($"Prelude loading failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    private async Task LoadPreludeText(string prelude)
    {
        var lines = prelude.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            // Skip empty lines and comments
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('\\'))
            {
                continue;
            }

            // Skip inline comments ( ... )
            if (trimmed.StartsWith('(') && trimmed.EndsWith(')'))
            {
                continue;
            }

            await EvalInternalAsync(trimmed);
        }
    }
}