using System.Collections.Immutable;

namespace Forth.Core.Interpreter;

/// <summary>
/// Encapsulates the parameter stack for the Forth interpreter.
/// </summary>
internal sealed class ForthStack : IReadOnlyList<object>
{
    private sealed class Node
    {
        public ForthValue Value { get; }
        public Node? Next { get; }
        public Node(ForthValue value, Node? next) => (Value, Next) = (value, next);
    }

    private Node? _head;
    private int _count;

    /// <summary>Gets the number of items currently on the stack.</summary>
    public int Count => _count;
    /// <summary>Index-based access (0-based from bottom of stack) returning boxed objects.</summary>
    public object this[int index]
    {
        get
        {
            if (index < 0 || index >= _count) throw new ArgumentOutOfRangeException(nameof(index));
            var node = _head;
            for (int i = 0; i < index; i++) node = node!.Next;
            return node!.Value.ToObject();
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection from bottom to top.
    /// </summary>
    public IEnumerator<object> GetEnumerator()
    {
        var node = _head;
        var stack = new Stack<object>();
        while (node != null)
        {
            stack.Push(node.Value.ToObject());
            node = node.Next;
        }
        while (stack.Count > 0)
        {
            yield return stack.Pop();
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>Push a value onto the stack.</summary>
    /// <param name="value">Value to push.</param>
    public void Push(ForthValue value)
    {
        _head = new Node(value, _head);
        _count++;
    }

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
        if (_count == 0) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.StackUnderflow, "Stack underflow");
        var v = _head!.Value;
        _head = _head.Next;
        _count--;
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
        if (_count == 0) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.StackUnderflow, "Stack underflow");
        return _head!.Value;
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
    public object NthFromTop(int n)
    {
        if (n <= 0 || n > _count) throw new System.ArgumentOutOfRangeException(nameof(n));
        Node? node = _head;
        for (int i = 1; i < n; i++) node = node?.Next;
        return node!.Value.ToObject();
    }
    /// <summary>Drops (removes) the top-most value.</summary>
    public void DropTop()
    {
        if (_count == 0) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.StackUnderflow, "Stack underflow");
        _head = _head!.Next;
        _count--;
    }
    /// <summary>Swaps the two top-most values.</summary>
    public void SwapTop2()
    {
        if (_count < 2) throw new Forth.Core.ForthException(Forth.Core.ForthErrorCode.StackUnderflow, "Stack underflow");
        var first = _head!.Value;
        var second = _head.Next!.Value;
        _head = new Node(second, new Node(first, _head.Next.Next));
    }

    /// <summary>Clears all items from the stack.</summary>
    public void Clear()
    {
        _head = null;
        _count = 0;
    }
}
