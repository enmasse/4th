using System;
using System.Collections.Generic;

namespace Forth.Core.Interpreter;

internal sealed class ControlFlowRuntime
{
    private readonly Stack<long> _loopIndexStack = new();
    private int _unloopPending;

    public void Push(long idx) => _loopIndexStack.Push(idx);

    public void Pop()
    {
        if (_loopIndexStack.Count == 0)
            throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "LOOP index stack underflow");
        _loopIndexStack.Pop();
    }
    public void PopMaybe()
    {
        if (_unloopPending > 0)
        {
            _unloopPending--;
            return;
        }
        Pop();
    }
    public void Unloop()
    {
        if (_loopIndexStack.Count == 0)
            throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "UNLOOP used outside DO...LOOP");
        _loopIndexStack.Pop();
        _unloopPending++;
    }
    public long Current()
    {
        if (_loopIndexStack.Count == 0)
            throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.CompileError, "I used outside DO...LOOP");
        return _loopIndexStack.Peek();
    }
}
