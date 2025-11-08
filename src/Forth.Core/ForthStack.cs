namespace Forth;

/// <summary>
/// Encapsulates the parameter stack for the Forth interpreter.
/// </summary>
internal sealed class ForthStack
{
    private readonly List<object> _items = new();

    public int Count => _items.Count;
    public object this[int index] => _items[index];
    public IReadOnlyList<object> AsReadOnly() => _items;

    public void Push(object value) => _items.Add(value);

    public object Pop()
    {
        var idx = _items.Count - 1;
        if (idx < 0) throw new ForthException(ForthErrorCode.StackUnderflow, "Stack underflow");
        var v = _items[idx];
        _items.RemoveAt(idx);
        return v;
    }

    public object Peek()
    {
        if (_items.Count == 0) throw new ForthException(ForthErrorCode.StackUnderflow, "Stack underflow");
        return _items[^1];
    }

    public object Top() => Peek();
    public object NthFromTop(int n) => _items[^n];
    public void DropTop() => _items.RemoveAt(_items.Count - 1);
    public void SwapTop2()
    {
        var last = _items.Count - 1;
        (_items[last-1], _items[last]) = (_items[last], _items[last-1]);
    }
}
