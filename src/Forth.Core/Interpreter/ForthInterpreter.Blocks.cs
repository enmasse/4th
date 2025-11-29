using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Forth.Core.Interpreter
{
    public partial class ForthInterpreter
    {
        // Block device simulation
        internal readonly Dictionary<int, string> _blocks = new();
        internal int _currentBlock = 0;
        internal string _blockBuffer = string.Empty;

        internal const int BlockSize = 1024;

        internal void EnsureBlockExists(int n)
        {
            if (!_blocks.ContainsKey(n))
            {
                _blocks[n] = new string(' ', BlockSize);
            }
        }

        internal string GetBlockBuffer(int n)
        {
            EnsureBlockExists(n);
            return _blocks[n];
        }

        internal void SetBlockBuffer(int n, string content)
        {
            if (content == null) content = string.Empty;
            if (content.Length > BlockSize) content = content.Substring(0, BlockSize);
            else if (content.Length < BlockSize) content = content.PadRight(BlockSize, ' ');
            _blocks[n] = content;
        }

        internal int GetCurrentBlockNumber() => _currentBlock;
        internal void SetCurrentBlockNumber(int n)
        {
            _currentBlock = n;
            _blockBuffer = GetBlockBuffer(n);
        }

        // Block-file backing (optional persistent storage)
        internal string? _blockFilePath;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("IDisposableAnalyzers", "IDISP008:Don't assign member with injected and created disposables", Justification = "FileStream/MemoryMappedFile ownership is managed explicitly by OpenBlockFile/CloseBlockFile.")]
        internal FileStream? _blockFileStream;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("IDisposableAnalyzers", "IDISP008:Don't assign member with injected and created disposables", Justification = "FileStream/MemoryMappedFile ownership is managed explicitly by OpenBlockFile/CloseBlockFile.")]
        internal MemoryMappedFile? _mmf;
        internal bool _useMmf = false;
        // Map block number -> reserved c-addr in interpreter memory
        internal readonly Dictionary<int, long> _blockAddrMap = new();
        internal readonly HashSet<int> _dirtyBlocks = new();
        // Cached per-block accessors for MMF zero-copy access
        internal readonly Dictionary<int, MemoryMappedViewAccessor> _mmfAccessors = new();

        // Per-block file mode
        internal string? _blockFileDir;
        internal bool _usePerBlockFiles = false;

        /// <summary>
        /// Open or create a block file for persistent storage. Attempts to create a memory-mapped
        /// file and falls back to FileStream when not available.
        /// </summary>
        internal void OpenBlockFile(string path, bool perBlock = false)
        {
            // Dispose previous backing if present to avoid leaking resources
            CloseBlockFile();

            // If perBlock is requested and path is a directory or ends with path separator, use directory mode
            _usePerBlockFiles = perBlock || Directory.Exists(path) || path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar);
            if (_usePerBlockFiles)
            {
                _blockFileDir = Path.GetFullPath(path);
                Directory.CreateDirectory(_blockFileDir);
                // disable mmf/single file mode
                _blockFilePath = null;
                // ensure any previous disposables were cleaned to satisfy analyzers
                try { _blockFileStream?.Dispose(); } catch { }
                _blockFileStream = null;
                try { _mmf?.Dispose(); } catch { }
                _mmf = null;
                _useMmf = false;
                return;
            }
 
            // Existing behavior for single-file backing
            _blockFilePath = Path.GetFullPath(path);
            var dir = Path.GetDirectoryName(_blockFilePath) ?? ".";
            Directory.CreateDirectory(dir);
            // Try to create MMF from path first (preferred). If not available, fall back to FileStream.
            try
            {
                // Dispose any previous before creating new
                try { _mmf?.Dispose(); } catch { }
                try { _blockFileStream?.Dispose(); } catch { }

                _mmf = MemoryMappedFile.CreateFromFile(_blockFilePath, FileMode.OpenOrCreate, null, 0, MemoryMappedFileAccess.ReadWrite);
                _useMmf = true;
                _blockFileStream = null;
            }
            catch
            {
                // Fall back to stream-only mode
                try { _mmf?.Dispose(); } catch { }
                _mmf = null;
                _useMmf = false;
                try { _blockFileStream?.Dispose(); } catch { }
                _blockFileStream = new FileStream(_blockFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            }
         }

        internal void CloseBlockFile()
        {
            // Per-block mode: nothing specific to do except clear dir if set
            if (_usePerBlockFiles)
            {
                _blockFileDir = null;
                _usePerBlockFiles = false;
                return;
            }
 
            // Flush and dispose cached accessors
            try
            {
                foreach (var acc in _mmfAccessors.Values) { try { acc?.Flush(); } catch { } }
            }
            catch { }
            try
            {
                foreach (var acc in _mmfAccessors.Values) { try { acc?.Dispose(); } catch { } }
            }
            catch { }
            _mmfAccessors.Clear();

            try
            {
                if (_mmf is not null)
                {
                    _mmf.Dispose();
                    _mmf = null;
                }
            }
            catch { }
            try
            {
                if (_blockFileStream is not null)
                {
                    _blockFileStream.Flush(true);
                    _blockFileStream.Dispose();
                    _blockFileStream = null;
                }
            }
            catch { }
            _useMmf = false;
            _blockFilePath = null;
        }

        /// <summary>
        /// Flush cached accessors and underlying file stream without closing the block file.
        /// </summary>
        internal void FlushBlockFile()
        {
            if (_usePerBlockFiles)
            {
                // Per-block files are written per-save; nothing to flush at single point.
                return;
            }

            try
            {
                foreach (var acc in _mmfAccessors.Values) { try { acc?.Flush(); } catch { } }
            }
            catch { }

            try
            {
                if (_blockFileStream is not null)
                {
                    lock (_blockFileStream)
                    {
                        _blockFileStream.Flush(true);
                    }
                }
            }
            catch { }
        }

        // Helper to obtain or create and cache an MMF accessor, disposing any previous accessor if replaced.
        private MemoryMappedViewAccessor CreateOrGetAccessor(int n, MemoryMappedFileAccess access)
        {
            if (_mmfAccessors.TryGetValue(n, out var existing) && existing is not null)
            {
                return existing;
            }

            var mmfLocal = _mmf;
            if (mmfLocal is null)
            {
                throw new ForthException(ForthErrorCode.CompileError, "MMF not available");
            }

            var acc = mmfLocal.CreateViewAccessor((long)n * BlockSize, BlockSize, access);
            if (_mmfAccessors.TryGetValue(n, out var prev) && prev is not null)
            {
                try { prev.Dispose(); } catch { }
            }
            _mmfAccessors[n] = acc;
            return acc;
        }
 
        internal void EnsureBlockExistsOnDisk(int n)
        {
            if (_usePerBlockFiles)
            {
                var fn = Path.Combine(_blockFileDir!, $"block-{n}.bin");
                if (!File.Exists(fn))
                {
                    // create zero-filled file of BlockSize
                    using var fs = new FileStream(fn, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                    var zero = new byte[BlockSize];
                    fs.Write(zero, 0, BlockSize);
                    fs.Flush(true);
                }
                return;
            }

            if (string.IsNullOrEmpty(_blockFilePath) || _blockFileStream is null)
            {
                EnsureBlockExists(n);
                return;
            }

            var required = (long)(n + 1) * BlockSize;
            if (_blockFileStream.Length < required)
            {
                _blockFileStream.SetLength(required);
                _blockFileStream.Flush(true);
            }
        }

        internal void LoadBlockFromBacking(int n, long addr)
        {
            if (_usePerBlockFiles)
            {
                var fn = Path.Combine(_blockFileDir!, $"block-{n}.bin");
                if (!File.Exists(fn))
                {
                    // fallback to in-memory
                    var content = GetBlockBuffer(n);
                    for (int k = 0; k < BlockSize; k++)
                    {
                        byte b = k < content.Length ? (byte)content[k] : (byte)0;
                        MemSet(addr + k, (long)b);
                    }
                    return;
                }

                var buffer = File.ReadAllBytes(fn);
                for (int k = 0; k < BlockSize; k++) MemSet(addr + k, (long)(k < buffer.Length ? buffer[k] : 0));
                return;
            }

            // existing single-file/MMF behavior
            if (string.IsNullOrEmpty(_blockFilePath) || _blockFileStream is null)
            {
                var content = GetBlockBuffer(n);
                for (int k = 0; k < BlockSize; k++)
                {
                    byte b = k < content.Length ? (byte)content[k] : (byte)0;
                    MemSet(addr + k, (long)b);
                }
                return;
            }

            if (_useMmf && _mmf is not null)
            {
                using var acc = _mmf.CreateViewAccessor((long)n * BlockSize, BlockSize, MemoryMappedFileAccess.Read);
                var buffer = new byte[BlockSize];
                acc.ReadArray(0, buffer, 0, BlockSize);
                for (int k = 0; k < BlockSize; k++) MemSet(addr + k, (long)buffer[k]);
            }
            else
            {
                lock (_blockFileStream)
                {
                    _blockFileStream.Seek((long)n * BlockSize, SeekOrigin.Begin);
                    var buffer = new byte[BlockSize];
                    var read = 0;
                    while (read < BlockSize)
                    {
                        var r = _blockFileStream.Read(buffer, read, BlockSize - read);
                        if (r <= 0) break;
                        read += r;
                    }
                    for (int k = 0; k < BlockSize; k++) MemSet(addr + k, (long)buffer[k]);
                }
            }
        }

        internal void SaveBlockToBacking_Suppressor(int n, long addr, int u) => SaveBlockToBacking(n, addr, u);

        internal void SaveBlockToBacking(int n, long addr, int u)
        {
            if (_usePerBlockFiles)
            {
                // always write full block to temp file then atomic rename
                var tmp = Path.Combine(_blockFileDir!, $"block-{n}.bin.tmp");
                var dst = Path.Combine(_blockFileDir!, $"block-{n}.bin");
                var buffer = new byte[BlockSize];
                for (int k = 0; k < BlockSize; k++) { MemTryGet(addr + k, out var v); buffer[k] = (byte)(v & 0xFF); }
                // write to tmp
                using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    fs.Write(buffer, 0, BlockSize);
                    fs.Flush(true);
                }
                // atomic replace
                File.Replace(tmp, dst, null);
                _dirtyBlocks.Remove(n);
                return;
            }

            // existing behavior for single-file/MMF
            if (string.IsNullOrEmpty(_blockFilePath) || _blockFileStream is null)
            {
                var sb = new System.Text.StringBuilder(u);
                for (int k = 0; k < u; k++) { MemTryGet(addr + k, out var v); sb.Append((char)(v & 0xFF)); }
                SetBlockBuffer(n, sb.ToString());
                return;
            }

            // For atomic full-block semantics, always write the full BlockSize.
            var bufferFull = new byte[BlockSize];
            for (int k = 0; k < BlockSize; k++) { MemTryGet(addr + k, out var v); bufferFull[k] = (byte)(v & 0xFF); }

            if (_useMmf)
            {
                var mmfLocal = _mmf;
                if (mmfLocal is not null)
                {
                    using var acc = mmfLocal.CreateViewAccessor((long)n * BlockSize, BlockSize, MemoryMappedFileAccess.ReadWrite);
                    acc.WriteArray(0, bufferFull, 0, BlockSize);
                    acc.Flush();
                }
            }
            else
            {
                lock (_blockFileStream)
                {
                    _blockFileStream.Seek((long)n * BlockSize, SeekOrigin.Begin);
                    _blockFileStream.Write(bufferFull, 0, BlockSize);
                    _blockFileStream.Flush(true);
                }
            }
            _dirtyBlocks.Remove(n);
        }

        /// <summary>
        /// Number of currently mapped block address mappings.
        /// </summary>
        internal int BlockMappingCount => _blockAddrMap.Count;

        /// <summary>
        /// Number of cached MMF accessors.
        /// </summary>
        internal int MmfAccessorCount => _mmfAccessors.Count;

        /// <summary>
        /// Current block file path if single-file mode is active, otherwise null.
        /// </summary>
        internal string? BlockFilePath => _blockFilePath;

        /// <summary>
        /// Current block file directory if per-block mode is active, otherwise null.
        /// </summary>
        internal string? BlockFileDir => _blockFileDir;

        // LRU tracking for cached block-address / accessor entries to limit growth
        // private const int MaxCachedBlocks = 64; // configurable cap for cached blocks
        private readonly LinkedList<int> _blockLru = new(); // most-recent-first
        private readonly Dictionary<int, LinkedListNode<int>> _blockLruNodes = new();

        internal bool TryGetBlockForAddr(long addr, out int block, out int offset)
        {
            foreach (var kv in _blockAddrMap)
            {
                var b = kv.Key;
                var start = kv.Value;
                if (addr >= start && addr < start + BlockSize)
                {
                    block = b;
                    offset = (int)(addr - start);
                    // mark block as recently used
                    MarkBlockUsed(b);
                    return true;
                }
            }
            block = -1; offset = -1; return false;
        }

        internal void MemSet_Suppressor(long addr, long v) => MemSet(addr, v);

        internal void MemSet(long addr,long v)
         {
            // If using MMF and address belongs to a mapped block, write through accessor (zero-copy semantics)
            if (_useMmf && TryGetBlockForAddr(addr, out var blk, out var off))
            {
                try
                {
                    var mmfLocal = _mmf;
                    if (mmfLocal is not null)
                    {
                        using var acc = mmfLocal.CreateViewAccessor((long)blk * BlockSize, BlockSize, MemoryMappedFileAccess.ReadWrite);
                        acc.Write(off, (byte)(v & 0xFF));
                        // mark dirty for explicit flush if needed
                        _dirtyBlocks.Add(blk);
                        // track usage
                        MarkBlockUsed(blk);
                        EnforceBlockCacheLimit();
                        return;
                    }
                }
                catch
                {
                    // Fall back to in-memory store on any mmf error
                }
            }

            // Default behavior: write into interpreter memory cells
            _mem[addr] = v;
        }

        internal void MemTryGet_Suppressor(long addr, out long v) { MemTryGet(addr, out v); }

        internal void MemTryGet(long addr, out long v)
         {
            // If using MMF and address belongs to a mapped block, read from accessor
            if (_useMmf && TryGetBlockForAddr(addr, out var blk, out var off))
            {
                try
                {
                    var mmfLocal = _mmf;
                    if (mmfLocal is not null)
                    {
                        using var acc = mmfLocal.CreateViewAccessor((long)blk * BlockSize, BlockSize, MemoryMappedFileAccess.Read);
                         v = (long)acc.ReadByte(off);
                         // track usage
                         MarkBlockUsed(blk);
                         EnforceBlockCacheLimit();
                         return;
                    }
                }
                catch
                {
                    // Fall back to in-memory on error
                }
            }

            if (!_mem.TryGetValue(addr, out v)) v = 0L;
        }

        internal long GetOrAllocateBlockAddr(int n)
        {
            if (_blockAddrMap.TryGetValue(n, out var addr))
            {
                // existing mapping: update LRU and return
                MarkBlockUsed(n);
                return addr;
            }
            var start = _nextAddr;
            _nextAddr += BlockSize;
            _blockAddrMap[n] = start;
            // Initialize memory region to zero
            for (int k = 0; k < BlockSize; k++) MemSet(start + k, 0);
            // Mark as used and enforce cache limit
            MarkBlockUsed(n);
            EnforceBlockCacheLimit();
            return start;
        }

        private void MarkBlockUsed(int n)
        {
            if (_blockLruNodes.TryGetValue(n, out var node))
            {
                _blockLru.Remove(node);
                _blockLru.AddFirst(node);
            }
            else
            {
                var nd = _blockLru.AddFirst(n);
                _blockLruNodes[n] = nd;
            }
        }

        private void EnforceBlockCacheLimit()
        {
            while (_blockLru.Count > _maxCachedBlocks)
            {
                var last = _blockLru.Last!;
                var evict = last.Value;
                _blockLru.RemoveLast();
                _blockLruNodes.Remove(evict);
                try
                {
                    EvictBlock(evict);
                }
                catch
                {
                    // Swallow eviction errors to avoid breaking runtime; eviction best-effort
                }
            }
        }

        private void EvictBlock(int n)
        {
            if (!_blockAddrMap.TryGetValue(n, out var addr)) return;

            // Flush/save current block to backing store
            try
            {
                SaveBlockToBacking(n, addr, BlockSize);
            }
            catch { }

            // Clear dirty marker
            _dirtyBlocks.Remove(n);

            // Remove mapping and free memory cells to bound memory growth
            _blockAddrMap.Remove(n);
            for (int k = 0; k < BlockSize; k++)
            {
                var a = addr + k;
                if (_mem.ContainsKey(a)) _mem.Remove(a);
            }
        }

        internal void ClearBlockBuffers()
        {
            // Evict all currently mapped blocks
            var blocksToEvict = _blockAddrMap.Keys.ToArray();
            foreach (var n in blocksToEvict)
            {
                EvictBlock(n);
            }
            // Clear LRU tracking
            _blockLru.Clear();
            _blockLruNodes.Clear();
        }
    }
}
