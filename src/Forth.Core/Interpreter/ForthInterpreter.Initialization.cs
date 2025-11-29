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
    }

    private async Task LoadPreludeAsync()
    {
        // Try to load embedded prelude resource
        var asm = typeof(ForthInterpreter).Assembly;
        var resourceName = "Forth.Core.prelude.4th";
        using var stream = asm.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            using var reader = new System.IO.StreamReader(stream);
            var prelude = await reader.ReadToEndAsync();
            await LoadPreludeText(prelude);
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
        }
        _preludeLoaded = true;
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