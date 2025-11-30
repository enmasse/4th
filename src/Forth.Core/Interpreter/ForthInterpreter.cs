using Forth.Core.Binding;
using Forth.Core.Execution;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using Forth.Core.Modules;

namespace Forth.Core.Interpreter;

public partial class ForthInterpreter : IForthInterpreter
{
    internal readonly ForthStack _stack = new();
    internal readonly ForthStack _rstack = new();
    internal ImmutableDictionary<(string? Module, string Name), Word> _dict = CorePrimitives.Words;
    private readonly IForthIO _io;
    private bool _exitRequested;
    internal readonly Dictionary<long,long> _mem = new(); // internal - numeric storage
    internal readonly Dictionary<long,object> _objMem = new(); // internal - object storage (execution tokens, etc)
    internal long _nextAddr = 1; // internal

    internal long _heapPtr = 1000000L;
    internal Dictionary<long, (byte[] bytes, long size)> _heapAllocations = new();

    internal readonly Dictionary<string,long> _values = new(StringComparer.OrdinalIgnoreCase); // internal

    internal Dictionary<string, object>? _locals;
    internal List<string>? _currentLocals;

    private readonly ControlFlowRuntime _controlFlow = new();

    private readonly long _stateAddr;
    internal long StateAddr =>
        _stateAddr;
    private readonly long _baseAddr;
    internal long BaseAddr =>
        _baseAddr;

    private readonly long _sourceAddr;
    internal readonly long _inAddr;
    internal long SourceAddr => _sourceAddr;
    internal long InAddr => _inAddr;

    private readonly long _scrAddr;
    internal long ScrAddr => _scrAddr;

    private StringBuilder? _picBuf;

    // Diagnostics for cell stores (debug)
    internal long _lastStoreAddr;
    internal long _lastStoreValue;

    // LRU cache size configuration (new)
    private int _maxCachedBlocks = 64; // default 64, configurable via BlockCacheSize property or constructor
    /// <summary>
    /// Gets or sets the maximum number of cached block mappings/accessors in the interpreter's LRU.
    /// Setting this property will evict blocks if the new value is less than the current count.
    /// </summary>
    public int BlockCacheSize
    {
        get => _maxCachedBlocks;
        set
        {
            if (value < 1) value = 1;
            _maxCachedBlocks = value;
            EnforceBlockCacheLimit();
        }
    }

    /// <summary>
    /// Converts an object to a long value, supporting common numeric types.
    /// </summary>
    /// <param name="v">The object to convert.</param>
    /// <returns>The long value.</returns>
    /// <exception cref="ForthException">Thrown if the object is not a supported numeric type.</exception>
    public static long ToLong(object v) => v switch
    {
        long l => l,
        int i => i,
        short s => s,
        byte b => b,
        char c => c,
        bool bo => bo ? -1L : 0L,
        double d => (long)d,
        Word w when w.BodyAddr.HasValue => w.BodyAddr.Value,
        _ => throw new ForthException(ForthErrorCode.TypeError, $"Expected number, got {v?.GetType().Name ?? "null"}")
    };

    private sealed class ExitWordException : System.Exception { }
    internal sealed class BracketIfFrame : CompileFrame
    {
        public bool Skipping { get; set; }
        public bool SeenElse { get; set; }
        // Removed provisional BracketIfFrame and interpret-time skipping logic pending full ANS implementation
        // Separate list so we do not invoke abstract base implementation
        private readonly List<Func<ForthInterpreter, Task>> _list = new();
        public override List<Func<ForthInterpreter, Task>> GetCurrentList() => _list;
    }

    internal void RequestExit() => _exitRequested = true;

    internal void ThrowExit() => throw new ExitWordException();
}
