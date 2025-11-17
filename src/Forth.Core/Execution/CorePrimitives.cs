using Forth.Core.Binding;
using Forth.Core.Interpreter;
using System.Globalization;
using System.Text;
using Word = Forth.Core.Interpreter.ForthInterpreter.Word;
using FI = Forth.Core.Interpreter.ForthInterpreter;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace Forth.Core.Execution;

internal static class CorePrimitives
{
    private sealed class KeyComparer : IEqualityComparer<(string? Module, string Name)>
    {
        public bool Equals((string? Module, string Name) x, (string? Module, string Name) y)
        {
            var scomp = StringComparer.OrdinalIgnoreCase;
            return scomp.Equals(x.Module, y.Module) && scomp.Equals(x.Name, y.Name);
        }

        public int GetHashCode((string? Module, string Name) obj)
        {
            var scomp = StringComparer.OrdinalIgnoreCase;
            int h1 = obj.Module is null ? 0 : scomp.GetHashCode(obj.Module);
            int h2 = scomp.GetHashCode(obj.Name);
            return (h1 * 397) ^ h2;
        }
    }

    public static ImmutableDictionary<(string? Module, string Name), Word> Words => new Dictionary<(string? Module, string Name), Word>(new KeyComparer())
        {
            // + : ( n1 n2 -- sum ) add two numbers
            { (null, "+"), new(i =>
                {
                    i.EnsureStack(2,"+");
                    var b=ToLong(i.PopInternal());
                    var a=ToLong(i.PopInternal());
                    i.Push(a+b);
                }) },
            // - : ( n1 n2 -- diff ) subtract second from first
            { (null, "-"), new(i =>
                {
                    i.EnsureStack(2,"-");
                    var b=ToLong(i.PopInternal());
                    var a=ToLong(i.PopInternal());
                    i.Push(a-b);
                }) },
            // * : ( n1 n2 -- prod ) multiply two numbers
            { (null, "*"), new(i =>
                {
                    i.EnsureStack(2,"*");
                    var b=ToLong(i.PopInternal());
                    var a=ToLong(i.PopInternal());
                    i.Push(a*b);
                }) },
            // / : ( n1 n2 -- quotient ) integer division, throws on divide by zero
            { (null, "/"), new(i =>
                {
                    i.EnsureStack(2,"/");
                    var b=ToLong(i.PopInternal());
                    var a=ToLong(i.PopInternal());
                    if (b==0)
                        throw new ForthException(ForthErrorCode.DivideByZero, "Divide by zero");
                    i.Push(a/b);
                }) },
            // /MOD : ( n1 n2 -- rem quot ) compute quotient and remainder; pushes remainder then quotient
            { (null, "/MOD"), new(i =>
                {
                    i.EnsureStack(2,"/MOD");
                    var b=ToLong(i.PopInternal());
                    var a=ToLong(i.PopInternal());
                    if (b==0) throw new ForthException(ForthErrorCode.DivideByZero, "Divide by zero");
                    var quot=a/b;
                    var rem=a % b;
                    i.Push(rem);
                    i.Push(quot);
                }) },
            // MOD : ( n1 n2 -- rem ) remainder of integer division
            { (null, "MOD"), new(i =>
                {
                    i.EnsureStack(2,"MOD");
                    var b=ToLong(i.PopInternal());
                    var a=ToLong(i.PopInternal());
                    if (b==0) throw new ForthException(ForthErrorCode.DivideByZero, "Divide by zero");
                    i.Push(a % b);
                }) },
            // < : ( a b -- flag ) compare less-than, returns 1/0
            { (null, "<"), new(i =>
                {
                    i.EnsureStack(2,"<");
                    var b=ToLong(i.PopInternal());
                    var a=ToLong(i.PopInternal());
                    i.Push(a < b ? 1L : 0L);
                }) },
            // = : ( a b -- flag ) equality comparison, returns 1 if equal else 0
            { (null, "="), new(i =>
                {
                    i.EnsureStack(2,"=");
                    var b=ToLong(i.PopInternal());
                    var a=ToLong(i.PopInternal());
                    i.Push(a == b ? 1L : 0L);
                }) },
            // > : ( a b -- flag ) greater-than comparison, returns 1/0
            { (null, ">"), new(i =>
                {
                    i.EnsureStack(2,">");
                    var b=ToLong(i.PopInternal());
                    var a=ToLong(i.PopInternal());
                    i.Push(a > b ? 1L : 0L);
                }) },
            // 0= : ( n -- flag ) true if zero
            { (null, "0="), new(i =>
                {
                    i.EnsureStack(1,"0=");
                    var a=ToLong(i.PopInternal());
                    i.Push(a==0 ? 1L : 0L);
                }) },
            // 0<> : ( n -- flag ) true if non-zero
            { (null, "0<>"), new(i =>
                {
                    i.EnsureStack(1,"0<>");
                    var a=ToLong(i.PopInternal());
                    i.Push(a!=0 ? 1L : 0L);
                }) },
            // <> : ( a b -- flag ) not equal comparison
            { (null, "<>") , new(i =>
                {
                    i.EnsureStack(2,"<>");
                    var b=ToLong(i.PopInternal());
                    var a=ToLong(i.PopInternal());
                    i.Push(a!=b ? 1L : 0L);
                }) },
            // <= : ( a b -- flag ) less-or-equal comparison
            { (null, "<="), new(i =>
                {
                    i.EnsureStack(2,"<=");
                    var b=ToLong(i.PopInternal());
                    var a=ToLong(i.PopInternal());
                    i.Push(a<=b ? 1L : 0L);
                }) },
            // >= : ( a b -- flag ) greater-or-equal comparison
            { (null, ">="), new(i =>
                {
                    i.EnsureStack(2,">=");
                    var b=ToLong(i.PopInternal());
                    var a=ToLong(i.PopInternal());
                    i.Push(a>=b ? 1L : 0L);
                }) },
            // MIN : ( a b -- min ) push smaller of two
            { (null, "MIN"), new(i =>
                {
                    i.EnsureStack(2,"MIN");
                    var b=ToLong(i.PopInternal());
                    var a=ToLong(i.PopInternal());
                    i.Push(a<b ? a : b);
                }) },
            // MAX : ( a b -- max ) push larger of two
            { (null, "MAX"), new(i =>
                {
                    i.EnsureStack(2,"MAX");
                    var b=ToLong(i.PopInternal());
                    var a=ToLong(i.PopInternal());
                    i.Push(a>b ? a : b);
                }) },
            // ROT : ( a b c -- b c a ) rotate third element to top
            { (null, "ROT"), new(i =>
                {
                    i.EnsureStack(3,"ROT");
                    var c=i.PopInternal();
                    var b=i.PopInternal();
                    var a=i.PopInternal();
                    i.Push(b);
                    i.Push(c);
                    i.Push(a);
                }) },
            // -ROT : ( a b c -- c a b ) reverse rotation
            { (null, "-ROT"), new(i =>
                {
                    i.EnsureStack(3,"-ROT");
                    var c=i.PopInternal();
                    var b=i.PopInternal();
                    var a=i.PopInternal();
                    i.Push(c);
                    i.Push(a);
                    i.Push(b);
                }) },
            // @ : ( addr -- value ) fetch cell from memory
            { (null, "@"), new(i =>
                {
                    i.EnsureStack(1,"@");
                    var addr=ToLong(i.PopInternal());
                    i.MemTryGet(addr, out var v);
                    i.Push(v);
                }) },
            // ! : ( addr val -- ) store cell to memory
            { (null, "!"), new(i =>
                {
                    i.EnsureStack(2,"!");
                    var addr=ToLong(i.PopInternal());
                    var val=ToLong(i.PopInternal());
                    i.MemSet(addr,val);
                }) },
            // +! : ( addr add -- ) add value to memory cell
            { (null, "+!"), new(i =>
                {
                    i.EnsureStack(2,"+!");
                    var addr=ToLong(i.PopInternal());
                    var add=ToLong(i.PopInternal());
                    i.MemTryGet(addr, out var cur);
                    i.MemSet(addr, cur + add);
                }) },
            // C! : ( addr char -- ) store low byte to memory
            { (null, "C!"), new(i =>
                {
                    i.EnsureStack(2,"C!");
                    var addr=ToLong(i.PopInternal());
                    var val=ToLong(i.PopInternal());
                    var b=(long)((byte)val);
                    i.MemSet(addr, b);
                }) },
            // C@ : ( addr -- char ) fetch low byte from memory
            { (null, "C@"), new(i =>
                {
                    i.EnsureStack(1,"C@");
                    var addr=ToLong(i.PopInternal());
                    i.MemTryGet(addr, out var v);
                    i.Push((long)((byte)v));
                }) },
            // MOVE : ( src dst u -- ) memory block move, handles overlap correctly
            { (null, "MOVE"), new(i =>
                {
                    i.EnsureStack(3,"MOVE");
                    var u = ToLong(i.PopInternal());
                    var dst = ToLong(i.PopInternal());
                    var src = ToLong(i.PopInternal());
                    if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative MOVE length");
                    if (u == 0) return;
                    // Handle overlap: copy direction based on addresses
                    if (src < dst && src + u > dst)
                    {
                        for (long k = u - 1; k >= 0; k--)
                        {
                            i.MemTryGet(src + k, out var v);
                            i.MemSet(dst + k, (long)((byte)v));
                        }
                    }
                    else
                    {
                        for (long k = 0; k < u; k++)
                        {
                            i.MemTryGet(src + k, out var v);
                            i.MemSet(dst + k, (long)((byte)v));
                        }
                    }
                }) },
            // FILL : ( addr u char -- ) fill u bytes at addr with char
            { (null, "FILL"), new(i =>
                {
                    i.EnsureStack(3,"FILL");
                    var ch = ToLong(i.PopInternal());
                    var u = ToLong(i.PopInternal());
                    var addr = ToLong(i.PopInternal());
                    if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative FILL length");
                    var b = (long)((byte)ch);
                    for (long k = 0; k < u; k++) i.MemSet(addr + k, b);
                }) },
            // ERASE : ( addr u -- ) set u bytes at addr to zero
            { (null, "ERASE"), new(i =>
                {
                    i.EnsureStack(2,"ERASE");
                    var u = ToLong(i.PopInternal());
                    var addr = ToLong(i.PopInternal());
                    if (u < 0) throw new ForthException(ForthErrorCode.CompileError, "Negative ERASE length");
                    for (long k = 0; k < u; k++) i.MemSet(addr + k, 0);
                }) },
            // DUMP : ( addr u -- ) write u bytes as hex string for inspection
            { (null, "DUMP"), new(i =>
                {
                    i.EnsureStack(2,"DUMP");
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
                }) },
            // >NUMBER : ( str start consumed -- value remainderLen totalConsumed ) parse digits from string according to BASE
            { (null, ">NUMBER"), new(i =>
                {
                    i.EnsureStack(3,">NUMBER");
                    var consumed = (int)ToLong(i.PopInternal());
                    var start = (int)ToLong(i.PopInternal());
                    var obj = i.PopInternal();
                    if (obj is not string s)
                        throw new ForthException(ForthErrorCode.TypeError, ">NUMBER expects a string and two integers");
                    // skip leading whitespace from start
                    int idx = Math.Clamp(start, 0, s.Length);
                    while (idx < s.Length && char.IsWhiteSpace(s[idx])) idx++;
                    // current BASE
                    i.MemTryGet(i.BaseAddr, out var baseVal);
                    int b = baseVal <= 0 ? 10 : (int)baseVal;
                    long value = 0;
                    int digits = 0;
                    while (idx < s.Length)
                    {
                        int d;
                        char ch = s[idx];
                        if (ch >= '0' && ch <= '9') d = ch - '0';
                        else if (ch >= 'A' && ch <= 'Z') d = ch - 'A' + 10;
                        else if (ch >= 'a' && ch <= 'z') d = ch - 'a' + 10;
                        else break;
                        if (d >= b) break;
                        value = value * b + d;
                        idx++;
                        digits++;
                    }
                    int remainder = s.Length - idx;
                    i.Push(value);
                    i.Push((long)remainder);
                    i.Push((long)(consumed + digits));
                }) },
            // <# : pictured numeric output start
            { (null, "<#"), new(i => i.PicturedBegin()) },
            // HOLD : ( char -- ) push character into pictured output buffer
            { (null, "HOLD"), new(i =>
                {
                    i.EnsureStack(1,"HOLD");
                    var n=ToLong(i.PopInternal());
                    i.PicturedHold((char)(n & 0xFFFF));
                }) },
            // # : pictured output digit extraction, return remaining quotient
            { (null, "#"), new(i =>
                {
                    i.EnsureStack(1,"#");
                    var n = ToLong(i.PopInternal());
                    // For this implementation, pictured numeric output is decimal-only regardless of BASE
                    const long b = 10;
                    long u = n < 0 ? -n : n;
                    long rem = u % b;
                    long q = u / b;
                    i.PicturedHoldDigit(rem);
                    i.Push(q);
                }) },
            // #S : pictured output produce digits of a number onto pictured buffer
            { (null, "#S"), new(i =>
                {
                    i.EnsureStack(1,"#S");
                    var n = ToLong(i.PopInternal());
                    // Decimal-only pictured numeric output regardless of BASE
                    const long b = 10;
                    long u = n < 0 ? -n : n;
                    if (u == 0)
                    {
                        i.PicturedHoldDigit(0);
                        i.Push(0L);
                        return;
                    }
                    while (u > 0)
                    {
                        long rem = u % b;
                        i.PicturedHoldDigit(rem);
                        u /= b;
                    }
                    i.Push(0L);
                }) },
            // SIGN : pictured output sign handling (push '-' if negative)
            { (null, "SIGN"), new(i =>
                {
                    i.EnsureStack(1,"SIGN");
                    var n=ToLong(i.PopInternal());
                    if (n < 0)
                        i.PicturedHold('-');
                }) },
            // #> : pictured output finish and push resulting string
            { (null, "#>"), new(i =>
                {
                    var s=i.PicturedEnd();
                    i.Push(s);
                }) },
            // >R : ( x -- ) move top of data stack to return stack
            { (null, ">R"), new(i =>
                {
                    i.EnsureStack(1,">R");
                    var a=i.PopInternal();
                    i.RPush(a);
                }) },
            // R> : ( -- x ) move top of return stack to data stack
            { (null, "R>"), new(i =>
                {
                    if (i.RCount==0)
                        throw new ForthException(ForthErrorCode.StackUnderflow,"Return stack underflow in R>");
                    var a=i.RPop();
                    i.Push(a);
                }) },
            // 2>R : ( x1 x2 -- ) move two cells to return stack
            { (null, "2>R"), new(i =>
                {
                    i.EnsureStack(2,"2>R");
                    var b=i.PopInternal();
                    var a=i.PopInternal();
                    i.RPush(a);
                    i.RPush(b);
                }) },
            // 2R> : ( -- x1 x2 ) pop two cells from return stack onto data stack
            { (null, "2R>"), new(i =>
                {
                    if (i.RCount<2)
                        throw new ForthException(ForthErrorCode.StackUnderflow,"Return stack underflow in 2R>");
                    var b=i.RPop();
                    var a=i.RPop();
                    i.Push(a);
                    i.Push(b);
                }) },
            // DUP : duplicate top of stack
            { (null, "DUP"), new(i =>
                {
                    i.EnsureStack(1,"DUP");
                    i.Push(i.StackTop());
                }) },
            // 2DUP : duplicate top two stack items
            { (null, "2DUP"), new(i =>
                {
                    i.EnsureStack(2,"2DUP");
                    var a=i.StackNthFromTop(2);
                    var b=i.StackNthFromTop(1);
                    i.Push(a);
                    i.Push(b);
                }) },
            // DROP : remove top of stack
            { (null, "DROP"), new(i =>
                {
                    i.EnsureStack(1,"DROP");
                    i.DropTop();
                }) },
            // SWAP : swap top two stack items
            { (null, "SWAP"), new(i =>
                {
                    i.EnsureStack(2,"SWAP");
                    i.SwapTop2();
                }) },
            // 2SWAP : swap top two pairs on the stack
            { (null, "2SWAP"), new(i =>
                {
                    i.EnsureStack(4,"2SWAP");
                    var d=i.PopInternal();
                    var c=i.PopInternal();
                    var b=i.PopInternal();
                    var a=i.PopInternal();
                    i.Push(c);
                    i.Push(d);
                    i.Push(a);
                    i.Push(b);
                }) },
            // OVER : copy second item to top
            { (null, "OVER"), new(i =>
                {
                    i.EnsureStack(2,"OVER");
                    i.Push(i.StackNthFromTop(2));
                }) },
            // NEGATE : ( n -- -n ) negate number
            { (null, "NEGATE"), new(i =>
                {
                    i.EnsureStack(1,"NEGATE");
                    var a=ToLong(i.PopInternal());
                    i.Push(-a);
                }) },
            // PICK : ( ... u -- ... xu ) copy u-th item from top to top
            { (null, "PICK"), new(i =>
                {
                    i.EnsureStack(1,"PICK");
                    var n=ToLong(i.PopInternal());
                    if(n<0) throw new ForthException(ForthErrorCode.StackUnderflow,$"PICK: negative index {n}");
                    if(n>=i.Stack.Count) throw new ForthException(ForthErrorCode.StackUnderflow,$"PICK: index {n} exceeds stack depth {i.Stack.Count}");
                    i.Push(i.StackNthFromTop((int)n+1));
                }) },
            // Note: Additional convenience words are in prelude.4th

            // YIELD : yield execution to scheduler
            { (null, "YIELD"), new(async i => await Task.Yield()) },
            // BYE, QUIT : request interpreter exit
            { (null, "BYE"), new(i => i.RequestExit()) },
            { (null, "QUIT"), new(i => i.RequestExit()) },
            // ABORT : raise an abort exception
            { (null, "ABORT"), new(i => throw new ForthException(ForthErrorCode.Unknown, "ABORT")) },
            // . : ( n -- ) write number to output
            { (null, "."), new(i =>
                {
                    i.EnsureStack(1,".");
                    var n=ToLong(i.PopInternal());
                    i.WriteNumber(n);
                }) },
            // .S : show stack contents as list
            { (null, ".S"), new(i =>
                {
                    var items = i.Stack;
                    var sb = new StringBuilder();
                    sb.Append('<').Append(items.Count).Append("> ");
                    for (int idx = 0; idx < items.Count; idx++)
                    {
                        if (idx > 0) sb.Append(' ');
                        var o = items[idx];
                        switch (o)
                        {
                            case long l: sb.Append(l); break;
                            case int ii: sb.Append(ii); break;
                            case short s: sb.Append((long)s); break;
                            case byte b: sb.Append((long)b); break;
                            case char ch: sb.Append((int)ch); break;
                            case bool bo: sb.Append(bo ? 1 : 0); break;
                            default:
                                sb.Append(o?.ToString() ?? "null");
                                break;
                        }
                    }
                    i.WriteText(sb.ToString());
                }) },
            // CR : write newline
            { (null, "CR"), new(i => i.NewLine()) },
            // EMIT : ( char -- ) output character
            { (null, "EMIT"), new(i =>
                {
                    i.EnsureStack(1,"EMIT");
                    var n=ToLong(i.PopInternal());
                    char ch=(char)(n & 0xFFFF);
                    i.WriteText(ch.ToString());
                }) },
            // TYPE : ( s -- ) output a string
            { (null, "TYPE"), new(i =>
                {
                    i.EnsureStack(1,"TYPE");
                    var obj=i.PopInternal();
                    if (obj is string s)
                    {
                        i.WriteText(s);
                    }
                    else
                        throw new ForthException(ForthErrorCode.TypeError, "TYPE expects a string"); }) },
            // COUNT : for strings return string and length; for counted buffers return address+1 and length
            { (null, "COUNT"), new(i =>
                {
                    i.EnsureStack(1,"COUNT");
                    var obj = i.PopInternal();
                    switch (obj)
                    {
                        case string s:
                            i.Push(s);
                            i.Push((long)s.Length);
                            break;
                        case long addr:
                            i.MemTryGet(addr, out var v);
                            var len = (long)((byte)v);
                            i.Push(addr + 1);
                            i.Push(len);
                            break;
                        default:
                            throw new ForthException(ForthErrorCode.TypeError, "COUNT expects a string or address");
                    }
                }) },
            // AND, OR, XOR : bitwise operations
            { (null, "AND"), new(i =>
                {
                    i.EnsureStack(2,"AND");
                    var b=ToLong(i.PopInternal());
                    var a=ToLong(i.PopInternal());
                    i.Push(a & b);
                }) },
            { (null, "OR"), new(i =>
                {
                    i.EnsureStack(2,"OR");
                    var b=ToLong(i.PopInternal());
                    var a=ToLong(i.PopInternal());
                    i.Push(a | b);
                }) },
            { (null, "XOR"), new(i =>
                {
                    i.EnsureStack(2,"XOR");
                    var b=ToLong(i.PopInternal());
                    var a=ToLong(i.PopInternal());
                    i.Push(a ^ b);
                }) },
            // INVERT : bitwise complement
            { (null, "INVERT"), new(i =>
                {
                    i.EnsureStack(1,"INVERT");
                    var a=ToLong(i.PopInternal());
                    i.Push(~a);
                }) },
            // LSHIFT, RSHIFT : logical shifts using unsigned semantics
            { (null, "LSHIFT"), new(i =>
                {
                    i.EnsureStack(2,"LSHIFT");
                    var u=ToLong(i.PopInternal());
                    var x=ToLong(i.PopInternal());
                    i.Push((long)((ulong)x << (int)u));
                }) },
            { (null, "RSHIFT"), new(i =>
                {
                    i.EnsureStack(2,"RSHIFT");
                    var u=ToLong(i.PopInternal());
                    var x=ToLong(i.PopInternal());
                    i.Push((long)((ulong)x >> (int)u));
                }) },
            // EXIT : signal exit from current compiled word
            { (null, "EXIT"), new(i => i.ThrowExit()) },
            // UNLOOP : helper to unwind loop state
            { (null, "UNLOOP"), new(i => i.Unloop()) },
            // DEPTH : push current stack depth
            { (null, "DEPTH"), new(i => i.Push((long)i.Stack.Count)) },
            // RP@ : push return stack depth
            { (null, "RP@"), new(i => i.Push((long)i.RCount)) },
            // STATE : push address of STATE flag (compiling/interpreting)
            { (null, "STATE"), new(i => i.Push(i.StateAddr)) },
            // BASE : push address of BASE (numeric base storage)
            { (null, "BASE"), new(i => i.Push(i.BaseAddr)) },
            // DECIMAL, HEX : set numeric base
            { (null, "DECIMAL"), new(i => i.MemSet(i.BaseAddr, 10)) },
            { (null, "HEX"), new(i => i.MemSet(i.BaseAddr, 16)) },
            // I : push current loop index
            { (null, "I"), new(i => i.Push(i.CurrentLoopIndex())) },
            // EXECUTE : execute an execution token or (xt value) pair
            { (null, "EXECUTE"), new(async i =>
                {
                    i.EnsureStack(1,"EXECUTE");
                    var top = i.StackTop();
                    if (top is Word wTop)
                    {
                        i.PopInternal();
                        await wTop.ExecuteAsync(i).ConfigureAwait(false);
                        return;
                    }
                    if (i.Stack.Count >= 2 && i.StackNthFromTop(2) is Word wBelow)
                    {
                        var data = i.PopInternal(); // value
                        i.PopInternal();            // xt
                        await wBelow.ExecuteAsync(i).ConfigureAwait(false);
                        i.Push(data);
                        return;
                    }
                    throw new ForthException(ForthErrorCode.TypeError, "EXECUTE expects an execution token");
                }) },
            // CATCH : ( xt -- 0 | err ) execute xt and catch Forth exceptions, returning error code
            { (null, "CATCH"), new(async i =>
                {
                    i.EnsureStack(1,"CATCH");
                    var obj = i.PopInternal();
                    if (obj is not Word xt) throw new ForthException(ForthErrorCode.TypeError, "CATCH expects an execution token");
                    try
                    {
                        await xt.ExecuteAsync(i).ConfigureAwait(false);
                        i.Push(0L);
                    }
                    catch (Forth.Core.ForthException ex)
                    {
                        var codeVal = (long)ex.Code;
                        if (codeVal == 0) codeVal = 1;
                        i.Push(codeVal);
                    }
                    catch (Exception)
                    {
                        // Push a non-zero error code for non-Forth exceptions (1 since Unknown is 0)
                        i.Push(1L);
                    }
                }) },
            // THROW : ( err -- ) rethrow a non-zero error as a ForthException
            { (null, "THROW"), new(i =>
                {
                    i.EnsureStack(1,"THROW");
                    var err = ToLong(i.PopInternal());
                    if (err != 0)
                        throw new Forth.Core.ForthException((Forth.Core.ForthErrorCode)err, $"THROW {err}");
                }) },

            // WORDS : list all available word names
            { (null, "WORDS"), new(i =>
                {
                    var names = i.GetAllWordNames();
                    var sb = new StringBuilder();
                    bool first = true;
                    foreach (var n in names)
                    {
                        if (!first) sb.Append(' ');
                        first = false;
                        sb.Append(n);
                    }
                    i.WriteText(sb.ToString());
                }) },

            // */ : ( n1 n2 n3 -- n ) multiply-then-divide
            { (null, "*/"), new(i =>
                {
                    i.EnsureStack(3,"*/");
                    var d = ToLong(i.PopInternal());
                    var n2 = ToLong(i.PopInternal());
                    var n1 = ToLong(i.PopInternal());
                    if (d == 0) throw new ForthException(ForthErrorCode.DivideByZero, "Divide by zero");
                    var prod = n1 * n2;
                    i.Push(prod / d);
                }) },

            // LATEST : push execution token of most recently defined word
            { (null, "LATEST"), new(i =>
                {
                    var last = i._lastDefinedWord;
                    if (last is null) throw new ForthException(ForthErrorCode.UndefinedWord, "No latest word");
                    i.Push(last);
                }) },

            // : ( compile ) start a new word definition
            { (null, ":"), new(i =>
                {
                    var name = i.ReadNextTokenOrThrow("Expected name after ':'");
                    i.BeginDefinition(name);
                }) { IsImmediate = true, Name = ":" } },
            // ; ( immediate ) finish current definition
            { (null, ";"), new(i => i.FinishDefinition()) { IsImmediate = true, Name = ";" } },
            // IMMEDIATE : mark last defined word immediate
            { (null, "IMMEDIATE"), new(i =>
                {
                    if (i._lastDefinedWord is null)
                        throw new ForthException(ForthErrorCode.CompileError, "No recent definition to mark IMMEDIATE");
                    i._lastDefinedWord.IsImmediate = true;
                }) { IsImmediate = true, Name = "IMMEDIATE" } },
            // POSTPONE : compile a word invocation to be executed later; handles special compile-only tokens
            { (null, "POSTPONE"), new(async i =>
                {
                    var name = i.ReadNextTokenOrThrow("POSTPONE expects a name");
                    if (i.TryResolveWord(name, out var wpost) && wpost is not null)
                    {
                        if (wpost.IsImmediate)
                            await wpost.ExecuteAsync(i);
                        else i.CurrentList().Add(ii => wpost.ExecuteAsync(ii));
                        return;
                    }
                    switch (name.ToUpperInvariant())
                    {
                        case "IF": case "ELSE": case "THEN": case "BEGIN": case "WHILE": case "REPEAT": case "UNTIL":
                        case "DO": case "LOOP": case "LEAVE": case "LITERAL": case "[": case "]": case "'": case "RECURSE":
                            i._tokens!.Insert(i._tokenIndex, name);
                            return;
                    }
                    throw new ForthException(ForthErrorCode.UndefinedWord, $"Undefined word: {name}");
                }) { IsImmediate = true, Name = "POSTPONE" } },
            // ' : ( -- xt ) push execution token of named word or compile a push of xt
            { (null, "'"), new(i =>
                {
                    var name = i.ReadNextTokenOrThrow("Expected word after '");
                    if (!i.TryResolveWord(name, out var wt) || wt is null)
                        throw new ForthException(ForthErrorCode.UndefinedWord, $"Undefined word: {name}");
                    if (!i._isCompiling)
                        i.Push(wt);
                    else
                        i.CurrentList().Add(ii =>
                            {
                                ii.Push(wt);
                                return Task.CompletedTask;
                            });
                }) { IsImmediate = true, Name = "'" } },
            // LITERAL : compile literal value into definition
            { (null, "LITERAL"), new(i =>
                {
                    if (!i._isCompiling)
                        throw new ForthException(ForthErrorCode.CompileError, "LITERAL outside compilation");
                    i.EnsureStack(1,"LITERAL");
                    var val=i.PopInternal();
                    i.CurrentList().Add(ii=>
                        {
                            ii.Push(val);
                            return Task.CompletedTask;
                        });
                }) { IsImmediate = true, Name = "LITERAL" } },
            // [ : enter interpret mode during compilation
            { (null, "["), new(i =>
                {
                    i._isCompiling=false;
                    i._mem[i.StateAddr]=0;
                }) { IsImmediate = true, Name = "[" } },
            // ] : return to compile mode
            { (null, "]"), new(i =>
                {
                    i._isCompiling=true;
                    i._mem[i.StateAddr]=1;
                }) { IsImmediate = true, Name = "]" } },
            // IF/ELSE/THEN : compile-time control flow for conditional execution
            { (null, "IF"), new(i => i._controlStack.Push(new FI.IfFrame())) { IsImmediate = true, Name = "IF" } },
            { (null, "ELSE"), new(i =>
                {
                    if(i._controlStack.Count==0 || i._controlStack.Peek() is not FI.IfFrame ifr)
                        throw new ForthException(ForthErrorCode.CompileError, "ELSE without IF");
                    if(ifr.ElsePart is not null)
                        throw new ForthException(ForthErrorCode.CompileError, "Multiple ELSE");
                    ifr.ElsePart=new();
                    ifr.InElse=true;
                }) { IsImmediate = true, Name = "ELSE" } },
            { (null, "THEN"), new(i =>
                {
                    if(i._controlStack.Count==0 || i._controlStack.Peek() is not FI.IfFrame ifr)
                        throw new ForthException(ForthErrorCode.CompileError, "THEN without IF");
                    i._controlStack.Pop();
                    var thenPart=ifr.ThenPart;
                    var elsePart=ifr.ElsePart;
                    i.CurrentList().Add(async ii=>
                        {
                            i.EnsureStack(1,"IF");
                            var flag=ii.PopInternal();
                            if(ToBool(flag))
                                foreach(var a in thenPart)
                                    await a(ii);
                            else if(elsePart is not null)
                                foreach(var a in elsePart)
                                    await a(ii);
                        });
                }) { IsImmediate = true, Name = "THEN" } },
            // BEGIN/WHILE/REPEAT/UNTIL : compile-time loop constructs
            { (null, "BEGIN"), new(i => i._controlStack.Push(new FI.BeginFrame())) { IsImmediate = true, Name = "BEGIN" } },
            { (null, "WHILE"), new(i =>
                {
                    if(i._controlStack.Count==0 || i._controlStack.Peek() is not FI.BeginFrame bf)
                        throw new ForthException(ForthErrorCode.CompileError, "WHILE without BEGIN");
                    if(bf.InWhile)
                        throw new ForthException(ForthErrorCode.CompileError, "Multiple WHILE");
                    bf.InWhile=true;
                }) { IsImmediate = true, Name = "WHILE" } },
            { (null, "REPEAT"), new(i =>
                {
                    if(i._controlStack.Count==0 || i._controlStack.Peek() is not FI.BeginFrame bf)
                        throw new ForthException(ForthErrorCode.CompileError, "REPEAT without BEGIN");
                    if(!bf.InWhile)
                        throw new ForthException(ForthErrorCode.CompileError, "REPEAT requires WHILE");
                    i._controlStack.Pop();
                    var pre=bf.PrePart;
                    var mid=bf.MidPart;
                    i.CurrentList().Add(async ii=>
                        {
                            while(true)
                            {
                                foreach (var a in pre)
                                    await a(ii);
                                i.EnsureStack(1,"WHILE");
                                var flag=ii.PopInternal();
                                if(!ToBool(flag))
                                    break;
                                foreach(var b in mid)
                                    await b(ii);
                            }
                        });
                }) { IsImmediate = true, Name = "REPEAT" } },
            { (null, "UNTIL"), new(i =>
                {
                    if(i._controlStack.Count==0 || i._controlStack.Peek() is not FI.BeginFrame bf)
                        throw new ForthException(ForthErrorCode.CompileError, "UNTIL without BEGIN");
                    if(bf.InWhile)
                        throw new ForthException(ForthErrorCode.CompileError, "UNTIL after WHILE use REPEAT");
                    i._controlStack.Pop();
                    var body=bf.PrePart;
                    i.CurrentList().Add(async ii=>
                        {
                            while(true)
                            {
                                foreach (var a in body)
                                    await a(ii);
                                i.EnsureStack(1,"UNTIL");
                                var flag=ii.PopInternal();
                                if(ToBool(flag))
                                    break;
                            }
                        });
                }) { IsImmediate = true, Name = "UNTIL" } },
            // DO/LOOP/LEAVE : compile-time loop for counted loops
            { (null, "DO"), new(i => i._controlStack.Push(new FI.DoFrame())) { IsImmediate = true, Name = "DO" } },
            { (null, "LOOP"), new(i =>
                {
                    if(i._controlStack.Count==0 || i._controlStack.Peek() is not FI.DoFrame df)
                        throw new ForthException(ForthErrorCode.CompileError, "LOOP without DO");
                    i._controlStack.Pop();
                    var body=df.Body;
                    i.CurrentList().Add(async ii=>
                        {
                            i.EnsureStack(2,"DO");
                            var start=ToLong(ii.PopInternal());
                            var limit=ToLong(ii.PopInternal());
                            long step=start<=limit?1L:-1L;
                            for(long idx=start; idx!=limit; idx+=step)
                            {
                                ii.PushLoopIndex(idx);
                                try
                                {
                                    foreach(var a in body)
                                        await a(ii);} 
                                catch(FI.LoopLeaveException)
                                {
                                    break;
                                }
                                finally
                                {
                                    ii.PopLoopIndexMaybe();
                                }
                            }
                        });
                }) { IsImmediate = true, Name = "LOOP" } },
            { (null, "LEAVE"), new(i =>
                {
                    bool inside=false;
                    foreach(var f in i._controlStack)
                        if(f is FI.DoFrame)
                        {
                            inside=true;
                            break;
                        }
                        if(!inside)
                            throw new ForthException(ForthErrorCode.CompileError, "LEAVE outside DO...LOOP");
                        i.CurrentList().Add(ii=> throw new FI.LoopLeaveException());
                }) { IsImmediate = true, Name = "LEAVE" } },
            // MODULE/END-MODULE/USING : module and vocabulary management
            { (null, "MODULE"), new(i =>
                {
                    var name=i.ReadNextTokenOrThrow("Expected name after MODULE");
                    i._currentModule=name;
                    if(string.IsNullOrWhiteSpace(i._currentModule))
                        throw new ForthException(ForthErrorCode.CompileError, "Invalid module name");
                }) { IsImmediate = true, Name = "MODULE" } },
            { (null, "END-MODULE"), new(i => i._currentModule = null) { IsImmediate = true, Name = "END-MODULE" } },
            { (null, "USING"), new(i =>
                {
                    var m=i.ReadNextTokenOrThrow("Expected name after USING");
                    if(!i._usingModules.Contains(m))
                        i._usingModules.Add(m);
                }) { IsImmediate = true, Name = "USING" } },
            // LOAD-ASM, LOAD-ASM-TYPE : load native assembly methods as Forth words
            { (null, "LOAD-ASM"), new(i =>
                {
                    var path=i.ReadNextTokenOrThrow("Expected path after LOAD-ASM");
                    var count=AssemblyWordLoader.Load(i,path);
                    i.Push((long)count);
                }) { IsImmediate = true, Name = "LOAD-ASM" } },
            { (null, "LOAD-ASM-TYPE"), new(i =>
                {
                    var tn=i.ReadNextTokenOrThrow("Expected type after LOAD-ASM-TYPE");
                    Type? t=Type.GetType(tn,false,false);
                    if(t==null)
                        foreach(var asm in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            t=asm.GetType(tn,false,false);
                            if(t!=null) break;
                        }
                        if(t==null)
                            throw new ForthException(ForthErrorCode.CompileError, $"Type not found: {tn}");
                    var count=i.LoadAssemblyWords(t.Assembly);
                    i.Push((long)count);
                }) { IsImmediate = true, Name = "LOAD-ASM-TYPE" } },
            // CREATE : create a new dictionary entry with allocated address
            { (null, "CREATE"), new(i =>
                {
                    var name=i.ReadNextTokenOrThrow("Expected name after CREATE");
                    if (string.IsNullOrWhiteSpace(name))
                        throw new ForthException(ForthErrorCode.CompileError, "Invalid name for CREATE");
                    var addr=i._nextAddr;
                    i._lastCreatedName=name;
                    i._lastCreatedAddr=addr;
                    i._dict = i._dict.SetItem((i._currentModule, name), new Word(ii=> ii.Push(addr)) { Name = name, Module = i._currentModule });
                    i.RegisterDefinition(name);
                }) { IsImmediate = true, Name = "CREATE" } },
            // , : compile-time comma store cell into dictionary
            { (null, ","), new(i =>
                {
                    i.EnsureStack(1,",");
                    var v=ToLong(i.PopInternal());
                    i._mem[i._nextAddr++]=v;
                }) { IsImmediate = true, Name = "," } },
            // DOES> : begin collecting DOES> clause for last CREATE
            { (null, "DOES>"), new(i =>
                {
                    if(string.IsNullOrEmpty(i._lastCreatedName))
                        throw new ForthException(ForthErrorCode.CompileError, "DOES> without CREATE");
                    i._doesCollecting=true;
                    i._doesTokens=new List<string>();
                }) { IsImmediate = true, Name = "DOES>" } },
            // ALLOT : reserve n cells in dictionary
            { (null, "ALLOT"), new(i =>
                {
                    i.EnsureStack(1,"ALLOT");
                    var cells=ToLong(i.PopInternal());
                    if(cells<0)
                        throw new ForthException(ForthErrorCode.CompileError, "Negative ALLOT size");
                    for(long k=0;k<cells;k++)
                        i._mem[i._nextAddr++]=0;
                }) { IsImmediate = true, Name = "ALLOT" } },
            // VARIABLE : create a named variable (address) in dictionary
            { (null, "VARIABLE"), new(i =>
                {
                    var name=i.ReadNextTokenOrThrow("Expected name after VARIABLE");
                    var addr=i._nextAddr++; i._mem[addr]=0;
                    i._dict = i._dict.SetItem((i._currentModule, name), new Word(ii=> ii.Push(addr)) { Name = name, Module = i._currentModule });
                    i.RegisterDefinition(name);
                }) { IsImmediate = true, Name = "VARIABLE" } },
            // CONSTANT : define a named constant pushing its value
            { (null, "CONSTANT"), new(i =>
                {
                    var name=i.ReadNextTokenOrThrow("Expected name after CONSTANT");
                    i.EnsureStack(1,"CONSTANT");
                    var val=i.PopInternal();
                    i._dict = i._dict.SetItem((i._currentModule, name), new Word(ii=> ii.Push(val)) { Name = name, Module = i._currentModule });
                    i.RegisterDefinition(name);
                }) { IsImmediate = true, Name = "CONSTANT" } },
            // VALUE / TO : mutable named values and assignment
            { (null, "VALUE"), new(i =>
                {
                    var name=i.ReadNextTokenOrThrow("Expected name after VALUE");
                    if(!i._values.ContainsKey(name))
                        i._values[name]=0;
                    i._dict = i._dict.SetItem((i._currentModule, name), new Word(ii=> ii.Push(ii.ValueGet(name))) { Name = name, Module = i._currentModule });
                    i.RegisterDefinition(name);
                }) { IsImmediate = true, Name = "VALUE" } },
            { (null, "TO"), new(i =>
                {
                    i.EnsureStack(1,"TO");
                    var name=i.ReadNextTokenOrThrow("Expected name after TO");
                    var vv=ToLong(i.PopInternal());
                    i.ValueSet(name,vv);
                }) { IsImmediate = true, Name = "TO" } },
            // DEFER / IS : create deferred words and set their target
            { (null, "DEFER"), new(i =>
                {
                    var name=i.ReadNextTokenOrThrow("Expected name after DEFER");
                    i._deferred[name]=null;
                    i._dict = i._dict.SetItem((i._currentModule, name), new Word(async ii =>
                        {
                            if(!ii._deferred.TryGetValue(name,out var target) || target is null)
                                throw new ForthException(ForthErrorCode.UndefinedWord, $"Deferred word not set: {name}");
                            await target.ExecuteAsync(ii);
                        }) { Name = name, Module = i._currentModule });
                    i.RegisterDefinition(name);
                }) { IsImmediate = true, Name = "DEFER" } },
            { (null, "IS"), new(i =>
                {
                    i.EnsureStack(1,"IS");
                    var name=i.ReadNextTokenOrThrow("Expected deferred name after IS");
                    var xtObj=i.PopInternal();
                    if(xtObj is not Word xt)
                        throw new ForthException(ForthErrorCode.TypeError, "IS expects an execution token");
                    if(!i._deferred.ContainsKey(name))
                        throw new ForthException(ForthErrorCode.UndefinedWord, $"No such deferred: {name}");
                    i._deferred[name]=xt;
                }) { IsImmediate = true, Name = "IS" } },
            // SEE : decompile or show source for a word
            { (null, "SEE"), new(i =>
                {
                    var name=i.ReadNextTokenOrThrow("Expected name after SEE");
                    var plain=name;
                    var cidx=name.IndexOf(':');
                    if(cidx>0)
                        plain=name[(cidx+1)..];
                    var text=i._decompile.TryGetValue(plain,out var s) ? s : $": {plain} ;";
                    i.WriteText(text);
                }) { IsImmediate = true, Name = "SEE" } },
            // CHAR : return numeric code of next token's first char
            { (null, "CHAR"), new(i =>
                {
                    var s=i.ReadNextTokenOrThrow("Expected char after CHAR");
                    if(!i._isCompiling)
                        i.Push(s.Length>0?(long)s[0]:0L);
                    else
                        i.CurrentList().Add(ii=>
                            {
                                ii.Push(s.Length>0?(long)s[0]:0L);
                                return Task.CompletedTask;
                            });
                }) { IsImmediate = true, Name = "CHAR" } },
            // S" and S : push quoted string at compile or runtime
            { (null, "S\""), new(i =>
                {
                    var next=i.ReadNextTokenOrThrow("Expected text after S\"");
                    if(next.Length<2 || next[0] != '"' || next[^1] != '"')
                        throw new ForthException(ForthErrorCode.CompileError, "S\" expects quoted token");
                    var str=next[1..^1];
                    if(!i._isCompiling)
                        i.Push(str);
                    else
                        i.CurrentList().Add(ii=>
                            {
                                ii.Push(str);
                                return Task.CompletedTask;
                            });
                }) { IsImmediate = true, Name = "S\"" } },
            { (null, "S"), new(i =>
                {
                    var next=i.ReadNextTokenOrThrow("Expected text after S");
                    if(next.Length<2 || next[0] != '"' || next[^1] != '"')
                        throw new ForthException(ForthErrorCode.CompileError, "S expects quoted token");
                    var str=next[1..^1];
                    if(!i._isCompiling)
                        i.Push(str);
                    else
                        i.CurrentList().Add(ii=>
                            {
                                ii.Push(str);
                                return Task.CompletedTask;
                            });
                }) { IsImmediate = true, Name = "S" } },
            // ." : compile-time or immediate print of quoted string
            { (null, ".\""), new(i =>
                {
                    var next=i.ReadNextTokenOrThrow("Expected text after .\"");
                    if(next.Length<2 || next[0] != '"' || next[^1] != '"')
                        throw new ForthException(ForthErrorCode.CompileError, ".\" expects quoted token");
                    var str=next[1..^1].TrimStart();
                    if(!i._isCompiling)
                    {
                        i.WriteText(str);
                    }
                    else
                    {
                        i.CurrentList().Add(ii=>
                            {
                                ii.WriteText(str);
                                return Task.CompletedTask;
                            });
                    }
                }) { IsImmediate = true, Name = ".\"" } },
            // BIND : bind a CLR method as a Forth word
            { (null, "BIND"), new(i =>
                {
                    var typeName=i.ReadNextTokenOrThrow("type after BIND");
                    var methodName=i.ReadNextTokenOrThrow("method after BIND");
                    var argToken=i.ReadNextTokenOrThrow("arg count after BIND");
                    if(!int.TryParse(argToken,NumberStyles.Integer,CultureInfo.InvariantCulture,out var argCount))
                        throw new ForthException(ForthErrorCode.CompileError, "Invalid arg count");
                    var forthName=i.ReadNextTokenOrThrow("forth name after BIND");
                    var bound = ClrBinder.CreateBoundWord(typeName,methodName,argCount);
                    i._dict = i._dict.SetItem((i._currentModule, forthName), bound);
                    i.RegisterDefinition(forthName);
                }) { IsImmediate = true, Name = "BIND" } },
            // FORGET : remove a word from dictionary
            { (null, "FORGET"), new(i =>
                {
                    var name=i.ReadNextTokenOrThrow("Expected name after FORGET");
                    i.ForgetWord(name);
                }) { IsImmediate = true, Name = "FORGET" } },
            // TASK? : check if object is a completed Task
            { (null, "TASK?"), new(i =>
                {
                    i.EnsureStack(1,"TASK?");
                    var obj=i.PopInternal();
                    long flag = obj is Task t && t.IsCompleted ? 1L : 0L; i.Push(flag);
                }) { Name = "TASK?" } },
            // AWAIT : await a Task and push its result if any
            { (null, "AWAIT"), new(async i => {
                i.EnsureStack(1,"AWAIT");
                var obj = i.PopInternal();
                switch(obj)
                {
                    case Task t:
                        // This will throw if the task is faulted
                        await t.ConfigureAwait(false);

                        var taskType = t.GetType();
                        if (taskType.IsGenericType)
                        {
                            var resultProperty = taskType.GetProperty("Result");
                            if (resultProperty != null && resultProperty.CanRead)
                            {
                                var result = resultProperty.GetValue(t);
                                if (result != null)
                                {
                                    var resultType = result.GetType();
                                    // Skip VoidTaskResult - it's the internal marker for Task (non-generic)
                                    if (resultType.Name == "VoidTaskResult")
                                        break;
                                    
                                    // Convert result to Forth-compatible type (same as ClrBinder.ForthInterpreterPush)
                                    switch (result)
                                    {
                                        case int iv: i.Push((long)iv); break;
                                        case long lv: i.Push(lv); break;
                                        case short sv: i.Push((long)sv); break;
                                        case byte bv: i.Push((long)bv); break;
                                        case char cv: i.Push((long)cv); break;
                                        case bool bov: i.Push(bov ? 1L : 0L); break;
                                        default: i.Push(result); break;
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        throw new ForthException(ForthErrorCode.CompileError, "AWAIT expects a Task or ValueTask");
                }
            }) { Name = "AWAIT" } },

            // JOIN is an idiomatic alias for AWAIT in Forth concurrency
            { (null, "JOIN"), new(async i => {
                // Delegate to AWAIT behavior
                i.EnsureStack(1,"JOIN");
                i._dict.TryGetValue((null, "AWAIT"), out var awaitWord);
                if (awaitWord is null)
                    throw new ForthException(ForthErrorCode.UndefinedWord, "AWAIT not found for JOIN");
                await awaitWord.ExecuteAsync(i);
            }) { Name = "JOIN" } },

            // SPAWN: ( xt -- task ) start xt on a background Task without result
            { (null, "SPAWN"), new(i => {
                i.EnsureStack(1, "SPAWN");
                var obj = i.PopInternal();
                if (obj is not Word xt)
                    throw new ForthException(ForthErrorCode.TypeError, "SPAWN expects an execution token");

                // Capture parent snapshot
                var snapshot = i.CreateMarkerSnapshot();

                var task = Task.Run(async () =>
                    { 
                        // Create child interpreter with parent's snapshot
                        var child = new ForthInterpreter(snapshot);
                        await xt.ExecuteAsync(child).ConfigureAwait(false);
                    });
                i.Push(task);
            }) { Name = "SPAWN" } },

            // FUTURE: ( xt -- task ) run xt on a fresh interpreter and return its top-of-stack as the task result
            { (null, "FUTURE"), new(i =>
                {
                    i.EnsureStack(1,"FUTURE");
                    var obj = i.PopInternal();
                    if (obj is not Word xt)
                        throw new ForthException(ForthErrorCode.TypeError, "FUTURE expects an execution token");
                
                    // Capture parent snapshot
                    var snapshot = i.CreateMarkerSnapshot();

                    var task = Task.Run(async () =>
                    {
                        // Create child interpreter with parent's snapshot
                        var child = new ForthInterpreter(snapshot);
                        await xt.ExecuteAsync(child).ConfigureAwait(false);
                        // If child left values, take top as result
                        return child.Stack.Count > 0 ? child.Pop() : null;
                    });
                    i.Push(task);
                }) { Name = "FUTURE" } },

            // TASK is a synonym for FUTURE for brevity, with same semantics
            { (null, "TASK"), new(i =>
                {
                    i.EnsureStack(1,"TASK");
                    var obj = i.PopInternal();
                    if (obj is not Word xt)
                        throw new ForthException(ForthErrorCode.TypeError, "TASK expects an execution token");
                
                    // Capture parent snapshot
                    var snapshot = i.CreateMarkerSnapshot();

                    var task = Task.Run(async () =>
                    {
                        // Create child interpreter with parent's snapshot
                        var child = new ForthInterpreter(snapshot);
                        await xt.ExecuteAsync(child).ConfigureAwait(false);
                        return child.Stack.Count > 0 ? child.Pop() : null;
                    });
                    i.Push(task);
                }) { Name = "TASK" } },

            // RECURSE: compile a call to the word being defined
            { (null, "RECURSE"), new(i =>
                {
                    if (!i._isCompiling || string.IsNullOrEmpty(i._currentDefName))
                        throw new ForthException(ForthErrorCode.CompileError, "RECURSE outside of a definition");
                    if (!i.TryResolveWord(i._currentDefName, out var self) || self is null)
                    {
                        // If not yet resolvable, queue a placeholder that resolves at runtime by name lookup
                        var name = i._currentDefName;
                        i.CurrentList().Add(async ii => {
                            if (!ii.TryResolveWord(name, out var w) || w is null)
                                throw new ForthException(ForthErrorCode.UndefinedWord, $"Undefined self word: {name}");
                            await w.ExecuteAsync(ii);
                        });
                        return;
                    }
                    i.CurrentList().Add(async ii => await self.ExecuteAsync(ii));
                }) { IsImmediate = true, Name = "RECURSE" } },

            // MARKER: create a word that when executed restores interpreter to this point
            { (null, "MARKER"), new(i =>
                {
                    var name = i.ReadNextTokenOrThrow("Expected name after MARKER");
                    var snap = i.CreateMarkerSnapshot();
                    i._dict = i._dict.SetItem((i._currentModule, name), new Word(ii =>
                        {
                            ii.RestoreSnapshot(snap);
                            return Task.CompletedTask;
                        }) { Name = name, Module = i._currentModule });
                    i.RegisterDefinition(name);
                }) { IsImmediate = true, Name = "MARKER" } },
        }.ToImmutableDictionary();

    private static long ToLong(object v) =>
        ForthInterpreter.ToLongPublic(v);

    private static bool ToBool(object v) => v switch
    {
        bool b => b,
        long l => l != 0,
        int i => i != 0,
        short s => s != 0,
        byte b8 => b8 != 0,
        char c => c != '\0',
        _ => throw new ForthException(ForthErrorCode.TypeError, $"Expected boolean/number, got {v?.GetType().Name ?? "null"}")
    };

}
