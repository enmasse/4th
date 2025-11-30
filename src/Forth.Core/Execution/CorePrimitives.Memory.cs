using Forth.Core.Interpreter;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Forth.Core.Execution;

internal static partial class CorePrimitives
{
    [Primitive("@", HelpString = "@ ( addr -- value ) - fetch cell at address")]
    private static Task Prim_At(ForthInterpreter i) 
    { 
        i.EnsureStack(1, "@"); 
        var addr = ToLong(i.PopInternal()); 
        // Fetch as object (handles both execution tokens and numeric values)
        i.MemTryGetObject(addr, out object v); 
        i.Push(v); 
        return Task.CompletedTask; 
    }

    [Primitive("!", HelpString = "! ( x addr -- ) - store x at address")]
    private static Task Prim_Bang(ForthInterpreter i)
    {
        i.EnsureStack(2, "!");
        var addrObj = i.PopInternal();
        var valObj = i.PopInternal();

        // Accept addresses as numbers or words with a data-field address
        long addr = addrObj switch
        {
            Word w when w.BodyAddr.HasValue => w.BodyAddr.Value,
            _ => ToLong(addrObj)
        };
        
        // Store the value - if it's a Word (execution token), store as object
        // Otherwise convert to long
        if (valObj is Word)
        {
            i.MemSet(addr, valObj);
            i._lastStoreAddr = addr;
            i._lastStoreValue = 0; // Can't store object in long field
        }
        else
        {
            long valueToStore = ToLong(valObj);
            i.MemSet(addr, valueToStore);
            i._lastStoreAddr = addr;
            i._lastStoreValue = valueToStore;
        }
        return Task.CompletedTask;
    }

    // No helper; `!` does not execute words to derive values

    [Primitive("+!", HelpString = "+! ( x addr -- ) - add x to cell at addr")]
    private static Task Prim_PlusBang(ForthInterpreter i) { i.EnsureStack(2, "+!"); var addr = ToLong(i.PopInternal()); var add = ToLong(i.PopInternal()); i.MemTryGet(addr, out var cur); i.MemSet(addr, ToLong(cur) + add); return Task.CompletedTask; }

    [Primitive("C!", HelpString = "C! ( x addr -- ) - store low byte of x at addr")]
    private static Task Prim_CBang(ForthInterpreter i) { i.EnsureStack(2, "C!"); var addr = ToLong(i.PopInternal()); var val = ToLong(i.PopInternal()); var b = (long)((byte)val); i.MemSet(addr, b); return Task.CompletedTask; }

    [Primitive("C@", HelpString = "C@ ( addr -- byte ) - fetch low byte at address")]
    private static Task Prim_CAt(ForthInterpreter i) { i.EnsureStack(1, "C@"); var addr = ToLong(i.PopInternal()); i.MemTryGet(addr, out var v); i.Push((long)((byte)v)); return Task.CompletedTask; }

    [Primitive("MOVE", HelpString = "MOVE ( src dst u -- ) - copy u bytes from src to dst")]
    private static Task Prim_MOVE(ForthInterpreter i)
    {
        i.EnsureStack(3, "MOVE");
        var u = ToLong(i.PopInternal());
        var dst = ToLong(i.PopInternal());
        var src = ToLong(i.PopInternal());
        if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative MOVE length");
        if (u == 0) return Task.CompletedTask;
        if (src < dst && src + u > dst)
        {
            for (long k = u - 1; k >= 0; k--)
            {
                i.MemTryGet(src + k, out var v);
                i.MemSet(dst + k, (long)((byte)v));
            }
            return Task.CompletedTask;
        }
        for (long k = 0; k < u; k++)
        {
            i.MemTryGet(src + k, out var v);
            i.MemSet(dst + k, (long)((byte)v));
        }
        return Task.CompletedTask;
    }

    [Primitive("CMOVE", HelpString = "CMOVE ( c-addr1 c-addr2 u -- ) - copy u bytes from c-addr1 to c-addr2")]
    private static Task Prim_CMOVE(ForthInterpreter i)
    {
        i.EnsureStack(3, "CMOVE");
        var u = ToLong(i.PopInternal());
        var dst = ToLong(i.PopInternal());
        var src = ToLong(i.PopInternal());
        if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative CMOVE length");
        for (long k = 0; k < u; k++)
        {
            i.MemTryGet(src + k, out var v);
            i.MemSet(dst + k, (long)((byte)v));
        }
        return Task.CompletedTask;
    }

    [Primitive("CMOVE>", HelpString = "CMOVE> ( c-addr1 c-addr2 u -- ) - copy u bytes from c-addr1 to c-addr2, proceeding from high to low")]
    private static Task Prim_CMOVE_Greater(ForthInterpreter i)
    {
        i.EnsureStack(3, "CMOVE>");
        var u = ToLong(i.PopInternal());
        var dst = ToLong(i.PopInternal());
        var src = ToLong(i.PopInternal());
        if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative CMOVE> length");
        for (long k = u - 1; k >= 0; k--)
        {
            i.MemTryGet(src + k, out var v);
            i.MemSet(dst + k, (long)((byte)v));
        }
        return Task.CompletedTask;
    }

    [Primitive("FILL", HelpString = "FILL ( addr u ch -- ) - fill u bytes at addr with ch")]
    private static Task Prim_FILL(ForthInterpreter i) { i.EnsureStack(3, "FILL"); var ch = ToLong(i.PopInternal()); var u = ToLong(i.PopInternal()); var addr = ToLong(i.PopInternal()); if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative FILL length"); var b = (long)((byte)ch); for (long k = 0; k < u; k++) i.MemSet(addr + k, b); return Task.CompletedTask; }

    [Primitive("BLANK", HelpString = "BLANK ( addr u -- ) - fill u bytes at addr with space")]
    private static Task Prim_BLANK(ForthInterpreter i) { i.EnsureStack(2, "BLANK"); var u = ToLong(i.PopInternal()); var addr = ToLong(i.PopInternal()); if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative BLANK length"); for (long k = 0; k < u; k++) i.MemSet(addr + k, 32); return Task.CompletedTask; }

    [Primitive("ERASE", HelpString = "ERASE ( addr u -- ) - set u bytes at addr to zero")]
    private static Task Prim_ERASE(ForthInterpreter i) { i.EnsureStack(2, "ERASE"); var u = ToLong(i.PopInternal()); var addr = ToLong(i.PopInternal()); if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative ERASE length"); for (long k = 0; k < u; k++) i.MemSet(addr + k, 0); return Task.CompletedTask; }

    [Primitive("DUMP", HelpString = "DUMP ( addr u -- ) - print u bytes from addr in hex")]
    private static Task Prim_DUMP(ForthInterpreter i)
    {
        i.EnsureStack(2, "DUMP");
        var u = ToLong(i.PopInternal());
        var addr = ToLong(i.PopInternal());
        if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative DUMP length");
        var sb = new StringBuilder();
        for (long k = 0; k < u; k++)
        {
            if (k > 0) sb.Append(' ');
            i.MemTryGet(addr + k, out var v);
            var b = (byte)v;
            sb.Append(b.ToString("X2"));
        }
        i.WriteText(sb.ToString());
        return Task.CompletedTask;
    }

    [Primitive("COUNT", HelpString = "COUNT ( c-addr -- c-addr u ) - return counted string address and length")]
    private static Task Prim_COUNT(ForthInterpreter i)
    {
        i.EnsureStack(1, "COUNT");
        var obj = i.PopInternal();
        switch (obj)
        {
            case string s:
                i.Push(s);
                i.Push((long)s.Length);
                return Task.CompletedTask;
            case long addr:
                i.MemTryGet(addr, out var v);
                var len = (long)((byte)v);
                i.Push(addr + 1);
                i.Push(len);
                return Task.CompletedTask;
            default:
                throw new ForthException(ForthErrorCode.TypeError, "COUNT expects a string or address");
        }
    }

    [Primitive(",", HelpString = ", ( x -- ) - append cell to dictionary and advance here")]
    private static Task Prim_Comma(ForthInterpreter i) { i.EnsureStack(1, ","); var v = ToLong(i.PopInternal()); i._mem[i._nextAddr++] = v; return Task.CompletedTask; }

    [Primitive("C,", HelpString = "C, ( ch -- ) - append byte to dictionary and advance here")]
    private static Task Prim_CComma(ForthInterpreter i) { i.EnsureStack(1, "C,"); var v = ToLong(i.PopInternal()); i._mem[i._nextAddr++] = (long)((byte)v); return Task.CompletedTask; }

    [Primitive("2!", HelpString = "2! ( x1 x2 addr -- ) - store two cells at address")]
    private static Task Prim_2Bang(ForthInterpreter i)
    {
        i.EnsureStack(3, "2!");
        var addr = ToLong(i.PopInternal());
        var x2 = ToLong(i.PopInternal());
        var x1 = ToLong(i.PopInternal());
        i.MemSet(addr, x1);
        i.MemSet(addr + 1, x2);
        return Task.CompletedTask;
    }

    [Primitive("2@", HelpString = "2@ ( addr -- x1 x2 ) - fetch two cells from address")]
    private static Task Prim_2At(ForthInterpreter i)
    {
        i.EnsureStack(1, "2@");
        var addr = ToLong(i.PopInternal());
        i.MemTryGet(addr, out var x1);
        i.MemTryGet(addr + 1, out var x2);
        i.Push(x1);
        i.Push(x2);
        return Task.CompletedTask;
    }

    [Primitive("CELL+", HelpString = "CELL+ ( addr -- addr+1 ) - add cell size to address")]
    private static Task Prim_CellPlus(ForthInterpreter i)
    {
        i.EnsureStack(1, "CELL+");
        var addr = ToLong(i.PopInternal());
        i.Push(addr + 1);
        return Task.CompletedTask;
    }

    [Primitive("CELLS", HelpString = "CELLS ( n -- n*cellsize ) - multiply by cell size")]
    private static Task Prim_Cells(ForthInterpreter i)
    {
        i.EnsureStack(1, "CELLS");
        var n = ToLong(i.PopInternal());
        i.Push(n * 1); // cell size 1
        return Task.CompletedTask;
    }

    [Primitive("CHAR+", HelpString = "CHAR+ ( c-addr -- c-addr+1 ) - add char size to address")]
    private static Task Prim_CharPlus(ForthInterpreter i)
    {
        i.EnsureStack(1, "CHAR+");
        var addr = ToLong(i.PopInternal());
        i.Push(addr + 1);
        return Task.CompletedTask;
    }

    [Primitive("CHARS", HelpString = "CHARS ( n -- n*charsize ) - multiply by char size")]
    private static Task Prim_Chars(ForthInterpreter i)
    {
        i.EnsureStack(1, "CHARS");
        var n = ToLong(i.PopInternal());
        i.Push(n * 1); // char size 1
        return Task.CompletedTask;
    }

    [Primitive("ALIGN", HelpString = "ALIGN ( -- ) - align dictionary pointer to cell boundary")]
    private static Task Prim_Align(ForthInterpreter i)
    {
        // Since cell size 1, no-op
        return Task.CompletedTask;
    }

    [Primitive("ALLOT", HelpString = "ALLOT ( u -- ) - reserve u cells in dictionary")]
    private static Task Prim_ALLOT(ForthInterpreter i) { i.EnsureStack(1, "ALLOT"); var cells = ToLong(i.PopInternal()); if (cells < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative ALLOT size"); for (long k = 0; k < cells; k++) i._mem[i._nextAddr++] = 0; return Task.CompletedTask; }

    // HERE: push the current dictionary allocation pointer (address of next free cell)
    [Primitive("HERE", HelpString = "HERE ( -- addr ) - push current dictionary allocation pointer")]
    private static Task Prim_HERE(ForthInterpreter i) { i.Push(i._nextAddr); return Task.CompletedTask; }

    [Primitive("UNUSED", HelpString = "UNUSED ( -- u ) - return the number of cells remaining in the data space")]
    private static Task Prim_UNUSED(ForthInterpreter i) { i.Push(1000000L - i._nextAddr); return Task.CompletedTask; }

    [Primitive("PAD", HelpString = "PAD ( -- addr ) - push address of scratch pad buffer")]
    private static Task Prim_PAD(ForthInterpreter i) { i.Push(i._nextAddr + 256); return Task.CompletedTask; }

    [Primitive("SOURCE", HelpString = "SOURCE ( -- addr u ) - return address and length of current input buffer")]
    private static Task Prim_SOURCE(ForthInterpreter i)
    {
        var src = i.CurrentSource ?? string.Empty;
        var addr = i.AllocateCountedString(src);
        i.Push(addr);
        i.Push((long)src.Length);
        return Task.CompletedTask;
    }

    [Primitive(">IN", HelpString = ">IN ( -- addr ) - address of >IN index cell")]
    private static Task Prim_IN(ForthInterpreter i)
    {
        i.Push(i.InAddr);
        return Task.CompletedTask;
    }

#if DEBUG
        [Primitive("LAST-STORE", HelpString = "LAST-STORE ( -- addr val ) - diagnostics: last ! store")]
        private static Task Prim_LAST_STORE(ForthInterpreter i)
        {
            i.Push(i._lastStoreAddr);
            i.Push(i._lastStoreValue);
            return Task.CompletedTask;
        }
#endif
}
