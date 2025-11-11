using System.Threading.Tasks;
using Forth.Core.Interpreter;

namespace Forth.Core.Execution;

internal static class CorePrimitives
{
    public static void Install(ForthInterpreter interp, Dictionary<string, ForthInterpreter.Word> dict)
    {
        dict["+"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"+"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a+b); });
        dict["-"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"-"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a-b); });
        dict["*"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"*"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a*b); });
        dict["/"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"/"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); if (b==0) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.DivideByZero,"Divide by zero"); i.Push(a/b); });
        dict["/MOD"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"/MOD"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); if (b==0) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.DivideByZero,"Divide by zero"); var quot=a/b; var rem=a % b; i.Push(rem); i.Push(quot); });
        dict["MOD"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"MOD"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); if (b==0) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.DivideByZero,"Divide by zero"); i.Push(a % b); });
        dict["<"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"<"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a < b ? 1L : 0L); });
        dict["="] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"="); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a == b ? 1L : 0L); });
        dict[">"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,">"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a > b ? 1L : 0L); });
        dict["ROT"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,3,"ROT"); var c=i.PopInternal(); var b=i.PopInternal(); var a=i.PopInternal(); i.Push(b); i.Push(c); i.Push(a); });
        dict["-ROT"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,3,"-ROT"); var c=i.PopInternal(); var b=i.PopInternal(); var a=i.PopInternal(); i.Push(c); i.Push(a); i.Push(b); });
        dict["@"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"@"); var addr=ToLong(i.PopInternal()); i.MemTryGet(addr, out var v); i.Push(v); });
        dict["!"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"!"); var addr=ToLong(i.PopInternal()); var val=ToLong(i.PopInternal()); i.MemSet(addr,val); });
        dict["+!"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"+!"); var addr=ToLong(i.PopInternal()); var add=ToLong(i.PopInternal()); i.MemTryGet(addr, out var cur); i.MemSet(addr, cur + add); });
        dict[">R"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,">R"); var a=i.PopInternal(); i.RPush(a); });
        dict["R>"] = new ForthInterpreter.Word(i => { if (i.RCount==0) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.StackUnderflow,"Return stack underflow in R>"); var a=i.RPop(); i.Push(a); });
        dict["2>R"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"2>R"); var b=i.PopInternal(); var a=i.PopInternal(); i.RPush(a); i.RPush(b); });
        dict["2R>"] = new ForthInterpreter.Word(i => { if (i.RCount<2) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.StackUnderflow,"Return stack underflow in 2R>"); var b=i.RPop(); var a=i.RPop(); i.Push(a); i.Push(b); });
        dict["DUP"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"DUP"); i.Push(i.StackTop()); });
        dict["2DUP"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"2DUP"); var a=i.StackNthFromTop(2); var b=i.StackNthFromTop(1); i.Push(a); i.Push(b); });
        dict["DROP"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"DROP"); i.DropTop(); });
        dict["SWAP"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"SWAP"); i.SwapTop2(); });
        dict["2SWAP"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,4,"2SWAP"); var d=i.PopInternal(); var c=i.PopInternal(); var b=i.PopInternal(); var a=i.PopInternal(); i.Push(c); i.Push(d); i.Push(a); i.Push(b); });
        dict["OVER"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"OVER"); i.Push(i.StackNthFromTop(2)); });
        dict["NEGATE"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"NEGATE"); var a=ToLong(i.PopInternal()); i.Push(-a); });
        dict["SPAWN"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"SPAWN"); var obj=i.PopInternal(); if (obj is not Task t) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError,"SPAWN expects a Task"); i.Push(t); i.Push(t); });
        dict["YIELD"] = new ForthInterpreter.Word(async i => { await Task.Yield(); });
        dict["BYE"] = new ForthInterpreter.Word(i => { i.RequestExit(); });
        dict["QUIT"] = new ForthInterpreter.Word(i => { i.RequestExit(); });
        dict["."] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"."); var n=ToLong(i.PopInternal()); i.WriteNumber(n); });
        dict["CR"] = new ForthInterpreter.Word(i => { i.NewLine(); });
        dict["EMIT"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"EMIT"); var n=ToLong(i.PopInternal()); char ch=(char)(n & 0xFFFF); i.WriteText(ch.ToString()); });
        dict["TYPE"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"TYPE"); var obj=i.PopInternal(); if (obj is string s) { i.WriteText(s); } else throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.TypeError,"TYPE expects a string"); });
        dict["AND"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"AND"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a & b); });
        dict["OR"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"OR"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a | b); });
        dict["XOR"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"XOR"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a ^ b); });
        dict["INVERT"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"INVERT"); var a=ToLong(i.PopInternal()); i.Push(~a); });
        dict["LSHIFT"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"LSHIFT"); var u=ToLong(i.PopInternal()); var x=ToLong(i.PopInternal()); i.Push((long)((ulong)x << (int)u)); });
        dict["RSHIFT"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"RSHIFT"); var u=ToLong(i.PopInternal()); var x=ToLong(i.PopInternal()); i.Push((long)((ulong)x >> (int)u)); });
        dict["EXIT"] = new ForthInterpreter.Word(i => { i.ThrowExit(); });
        // Introspection
        dict["DEPTH"] = new ForthInterpreter.Word(i => { i.Push((long)i.Stack.Count); });
        dict["RP@"] = new ForthInterpreter.Word(i => { i.Push((long)i.RCount); });
        // Loop index
        dict["I"] = new ForthInterpreter.Word(i => { i.Push(i.CurrentLoopIndex()); });
    }

    private static long ToLong(object v) => ForthInterpreter.ToLongPublic(v);
}
