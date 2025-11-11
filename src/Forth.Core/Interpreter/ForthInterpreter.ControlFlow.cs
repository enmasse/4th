using System;

namespace Forth.Core.Interpreter;

public partial class ForthInterpreter
{
    // DO..LOOP index stack for nested loops (innermost at top)
    private readonly Stack<long> _loopIndexStack = new();
    private int _unloopPending; // counts iterations where UNLOOP already removed loop index

    internal void PushLoopIndex(long idx) => _loopIndexStack.Push(idx);
    internal void PopLoopIndex()
    {
        if (_loopIndexStack.Count == 0)
            throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "LOOP index stack underflow");
        _loopIndexStack.Pop();
    }
    internal void PopLoopIndexMaybe()
    {
        if (_unloopPending > 0)
        {
            _unloopPending--;
            return;
        }
        PopLoopIndex();
    }
    internal void Unloop()
    {
        if (_loopIndexStack.Count == 0)
            throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "UNLOOP used outside DO...LOOP");
        _loopIndexStack.Pop();
        _unloopPending++;
    }
    internal long CurrentLoopIndex()
    {
        if (_loopIndexStack.Count == 0)
            throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "I used outside DO...LOOP");
        return _loopIndexStack.Peek();
    }
}
