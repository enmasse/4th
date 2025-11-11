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
        dict["<"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"<"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a < b ? 1L : 0L); });
        dict["="] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"="); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a == b ? 1L : 0L); });
        dict[">"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,">"); var b=ToLong(i.PopInternal()); var a=ToLong(i.PopInternal()); i.Push(a > b ? 1L : 0L); });
        dict["ROT"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,3,"ROT"); var c=i.PopInternal(); var b=i.PopInternal(); var a=i.PopInternal(); i.Push(b); i.Push(c); i.Push(a); });
        dict["-ROT"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,3,"-ROT"); var c=i.PopInternal(); var b=i.PopInternal(); var a=i.PopInternal(); i.Push(c); i.Push(a); i.Push(b); });
        dict["@"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"@"); var addr=ToLong(i.PopInternal()); i.MemTryGet(addr, out var v); i.Push(v); });
        dict["!"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"!"); var addr=ToLong(i.PopInternal()); var val=ToLong(i.PopInternal()); i.MemSet(addr,val); });
        dict["DUP"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"DUP"); i.Push(i.StackTop()); });
        dict["DROP"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"DROP"); i.DropTop(); });
        dict["SWAP"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"SWAP"); i.SwapTop2(); });
        dict["OVER"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,2,"OVER"); i.Push(i.StackNthFromTop(2)); });
        dict["NEGATE"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"NEGATE"); var a=ToLong(i.PopInternal()); i.Push(-a); });
        dict["SPAWN"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"SPAWN"); var obj=i.PopInternal(); if (obj is not Task t) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError,"SPAWN expects a Task"); i.Push(t); i.Push(t); });
        dict["YIELD"] = new ForthInterpreter.Word(async i => { await Task.Yield(); });
        dict["BYE"] = new ForthInterpreter.Word(i => { i.RequestExit(); });
        dict["QUIT"] = new ForthInterpreter.Word(i => { i.RequestExit(); });
        dict["."] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"."); var n=ToLong(i.PopInternal()); i.WriteNumber(n); });
        dict["CR"] = new ForthInterpreter.Word(i => { i.NewLine(); });
        dict["EMIT"] = new ForthInterpreter.Word(i => { ForthInterpreter.EnsureStack(i,1,"EMIT"); var n=ToLong(i.PopInternal()); char ch=(char)(n & 0xFFFF); i.WriteText(ch.ToString()); });
        dict["EXIT"] = new ForthInterpreter.Word(i => { i.ThrowExit(); });
        // Introspection
        dict["DEPTH"] = new ForthInterpreter.Word(i => { i.Push((long)i.Stack.Count); });
    }

    private static long ToLong(object v) => ForthInterpreter.ToLongPublic(v);
}
