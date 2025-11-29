using System.Text;

namespace Forth.Core.Interpreter;

// Partial: memory management, allocation, and string operations
public partial class ForthInterpreter
{
    internal long AllocateCountedString(string str)
    {
        var addr = _nextAddr;
        _mem[addr] = str.Length;
        for (int idx = 0; idx < str.Length; idx++)
            _mem[addr + 1 + idx] = (long)str[idx];
        _nextAddr = addr + 1 + str.Length;
        return addr;
    }

    internal long AllocateString(string str)
    {
        var addr = _nextAddr;
        for (int idx = 0; idx < str.Length; idx++)
            _mem[addr + idx] = (long)str[idx];
        _nextAddr += str.Length;
        return addr;
    }

    // Unified string reading helpers
    internal string ReadCountedString(long addr)
    {
        MemTryGet(addr, out var len);
        var l = (int)len;
        if (l <= 0) return string.Empty;
        var sb = new StringBuilder(l);
        for (int k = 0; k < l; k++)
        {
            MemTryGet(addr + 1 + k, out var chv);
            sb.Append((char)chv);
        }
        return sb.ToString();
    }

    internal string ReadMemoryString(long addr, long u)
    {
        if (u <= 0) return string.Empty;
        var sb = new StringBuilder((int)u);
        for (long k = 0; k < u; k++)
        {
            MemTryGet(addr + k, out var chv);
            sb.Append((char)chv);
        }
        return sb.ToString();
    }
}