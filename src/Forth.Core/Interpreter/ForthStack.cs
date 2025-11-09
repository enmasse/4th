namespace Forth;

/// <summary>
/// Encapsulates the parameter stack for the Forth interpreter.
/// </summary>
internal sealed class ForthStack
{
    private readonly List<object> _items = new();

    /// <summary>Gets the number of items currently on the stack.</summary>
    public int Count => _items.Count;
    /// <summary>Index-based access (0-based from bottom of stack).</summary>
    public object this[int index] => _items[index];
    /// <summary>Returns a read-only view of the stack items (top is last element).</summary>
    public IReadOnlyList<object> AsReadOnly() => _items;

    /// <summary>Push a value onto the stack.</summary>
    /// <param name="value">Value to push.</param>
    public void Push(object value) => _items.Add(value);

    /// <summary>Pop and return the top-most value.</summary>
    /// <exception cref="ForthException">Thrown when stack is empty.</exception>
    public object Pop()
    {
        var idx = _items.Count - 1;
        if (idx < 0) throw new ForthException(ForthErrorCode.StackUnderflow, "Stack underflow");
        var v = _items[idx];
        _items.RemoveAt(idx);
        return v;
    }

    /// <summary>Return the top-most value without removing it.</summary>
    /// <exception cref="ForthException">Thrown when stack is empty.</exception>
    public object Peek()
    {
        if (_items.Count == 0) throw new ForthException(ForthErrorCode.StackUnderflow, "Stack underflow");
        return _items[^1];
    }

    /// <summary>Alias for <see cref="Peek"/>.</summary>
    public object Top() => Peek();
    /// <summary>Returns the nth value from the top (1 = top).</summary>
    /// <param name="n">Position from top (1-based).</param>
    public object NthFromTop(int n) => _items[^n];
    /// <summary>Drops (removes) the top-most value.</summary>
    public void DropTop() => _items.RemoveAt(_items.Count - 1);
    /// <summary>Swaps the two top-most values.</summary>
    public void SwapTop2()
    {
        var last = _items.Count - 1;
        (_items[last-1], _items[last]) = (_items[last], _items[last-1]);
    }
}
