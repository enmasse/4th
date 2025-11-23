namespace Forth.Core.Interpreter;

/// <summary>
/// Encapsulates the parameter stack for the Forth interpreter.
/// </summary>
internal sealed class ForthStack
{
    private readonly List<ForthValue> _items = new();

    /// <summary>Gets the number of items currently on the stack.</summary>
    public int Count => _items.Count;
    /// <summary>Index-based access (0-based from bottom of stack).</summary>
    public ForthValue this[int index] => _items[index];
    /// <summary>Returns a read-only view of the stack items (top is last element).</summary>
    public IReadOnlyList<ForthValue> AsReadOnly() => _items;

    /// <summary>Push a value onto the stack.</summary>
    /// <param name="value">Value to push.</param>
    public void Push(ForthValue value) => _items.Add(value);

    /// <summary>Push a value onto the stack.</summary>
    /// <param name="value">Value to push.</param>
    public void Push(object value)
    {
        ForthValue fv = value switch
        {
            long l => ForthValue.FromLong(l),
            double d => ForthValue.FromDouble(d),
            string s => ForthValue.FromString(s),
            _ => ForthValue.FromObject(value)
        };
        Push(fv);
    }

    /// <summary>Pop and return the top-most value.</summary>
    /// <exception cref="ForthException">Thrown when stack is empty.</exception>
    public ForthValue PopValue()
    {
        var idx = _items.Count - 1;
        if (idx < 0) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.StackUnderflow, "Stack underflow");
        var v = _items[idx];
        _items.RemoveAt(idx);
        return v;
    }

    /// <summary>Pop and return the top-most value as object.</summary>
    /// <exception cref="ForthException">Thrown when stack is empty.</exception>
    public object Pop()
    {
        return PopValue().ToObject();
    }

    /// <summary>Return the top-most value without removing it.</summary>
    /// <exception cref="ForthException">Thrown when stack is empty.</exception>
    public ForthValue PeekValue()
    {
        if (_items.Count == 0) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.StackUnderflow, "Stack underflow");
        return _items[^1];
    }

    /// <summary>Return the top-most value without removing it.</summary>
    /// <exception cref="ForthException">Thrown when stack is empty.</exception>
    public object Peek()
    {
        return PeekValue().ToObject();
    }

    /// <summary>Alias for <see cref="Peek"/>.</summary>
    public object Top() => Peek();
    /// <summary>Returns the nth value from the top (1 = top).</summary>
    /// <param name="n">Position from top (1-based).</param>
    public object NthFromTop(int n) => _items[^n].ToObject();
    /// <summary>Drops (removes) the top-most value.</summary>
    public void DropTop() => _items.RemoveAt(_items.Count - 1);
    /// <summary>Swaps the two top-most values.</summary>
    public void SwapTop2()
    {
        var last = _items.Count - 1;
        (_items[last-1], _items[last]) = (_items[last], _items[last-1]);
    }
}
