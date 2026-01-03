using System.Collections.Generic;
using System.Text;

namespace Forth.Core.Interpreter;

// Partial: stack operations, data and return stacks
public partial class ForthInterpreter
{
    /// <summary>
    /// Gets a read-only snapshot of the data stack contents (bottom at index 0).
    /// </summary>
    public IReadOnlyList<object> Stack => _stack.ToList();

    /// <summary>
    /// Pushes a raw object onto the data stack.
    /// </summary>
    /// <param name="value">Value to push.</param>
    public void Push(object value)
    {
        _stack.Push(value);
    }

    /// <summary>
    /// Pops and returns the top item on the data stack.
    /// </summary>
    /// <returns>The popped value.</returns>
    public object Pop()
    {
        return _stack.Pop();
    }

    /// <summary>
    /// Peeks at the top item on the data stack without removing it.
    /// </summary>
    /// <returns>The top stack value.</returns>
    public object Peek()
    {
        return _stack.Peek();
    }

    internal object PopInternal()
    {
        return _stack.Pop();
    }

    internal void EnsureStack(int needed, string word)
    {
        if (_stack.Count < needed)
        {
            throw new ForthException(ForthErrorCode.StackUnderflow, $"Stack underflow in {word}");
        }
    }

    // Return stack helpers
    internal void RPush(object value)
    {
        ForthValue fv = value switch
        {
            long l => ForthValue.FromLong(l),
            double d => ForthValue.FromDouble(d),
            string s => ForthValue.FromString(s),
            _ => ForthValue.FromObject(value)
        };
        _rstack.Push(fv);
    }

    internal object RPop() =>
        _rstack.Pop();

    internal int RCount =>
        _rstack.Count;

    // Peek top of return stack without removing it
    internal object RTop() =>
        _rstack.Peek();

    internal object RNth(int n)
    {
        var arr = _rstack.ToArray();
        if (n >= arr.Length) throw new ForthException(ForthErrorCode.StackUnderflow, "Return stack underflow in RNth");
        return arr[arr.Length - 1 - n];
    }

    internal long ValueGet(string name) =>
        _values.TryGetValue(name, out var v) ? v : 0L;

    internal void ValueSet(string name, long v) =>
        _values[name] = v;

    internal object StackTop() =>
        _stack.Top();
    internal object StackNthFromTop(int n) =>
        _stack.NthFromTop(n);
    internal void DropTop() =>
        _stack.DropTop();
    internal void SwapTop2() =>
        _stack.SwapTop2();

    internal void PushLocal(string name)
    {
        if (_locals == null || !_locals.TryGetValue(name, out var val))
            throw new ForthException(ForthErrorCode.UndefinedWord, $"Local {name} not defined");
        Push(val);
    }

    internal void PicturedBegin() =>
        _picBuf = new StringBuilder();

    internal void PicturedHold(char ch)
    {
        _picBuf ??= new StringBuilder();
        _picBuf.Insert(0, ch);
    }

    internal void PicturedHoldDigit(long digit)
    {
        int d = (int)digit;
        char ch = (char)(d < 10 ? '0' + d : 'A' + (d - 10));
        PicturedHold(ch);
    }

    internal string PicturedEnd()
    {
        var s = _picBuf?.ToString() ?? string.Empty;
        _picBuf = null;
        return s;
    }
}

// Partial: stack operations and loop control
public partial class ForthInterpreter
{
    internal void PushLoopIndex(long idx) => _controlFlow.Push(idx);
    internal void PopLoopIndexMaybe() => _controlFlow.PopMaybe();
    internal void Unloop() => _controlFlow.Unloop();
    internal long CurrentLoopIndex() => _controlFlow.Current();
}
