using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Forth.Core.Interpreter
{
    public sealed class LazyDictionary<TKey, TValue> : Dictionary<TKey?, TValue>
    {
        private readonly Func<TKey?, TValue> _factory;

        // Dedicated storage for null key because base Dictionary may not handle null uniformly
        private bool _hasNullKeyValue;
        private TValue? _nullKeyValue;

        public LazyDictionary(Func<TKey?, TValue> factory) : base()
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public LazyDictionary(Func<TKey?, TValue> factory, IEqualityComparer<TKey?>? comparer) : base(comparer ?? EqualityComparer<TKey?>.Default)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        // Hide base indexer to provide lazy creation on get
        public new TValue this[TKey? key]
        {
            get
            {
                if (key is null)
                {
                    if (!_hasNullKeyValue)
                    {
                        _nullKeyValue = _factory(key);
                        _hasNullKeyValue = true;
                    }
                    return _nullKeyValue!;
                }

                if (!base.TryGetValue(key, out var val))
                {
                    val = _factory(key);
                    base[key] = val;
                }
                return val!;
            }
            set
            {
                if (key is null)
                {
                    _nullKeyValue = value;
                    _hasNullKeyValue = true;
                }
                else
                {
                    base[key] = value;
                }
            }
        }

        public new bool TryGetValue(TKey? key, out TValue value)
        {
            if (key is null)
            {
                if (_hasNullKeyValue)
                {
                    value = _nullKeyValue!;
                    return true;
                }
                value = default!;
                return false;
            }
            return base.TryGetValue(key, out value!);
        }

        public new bool ContainsKey(TKey? key)
        {
            if (key is null) return _hasNullKeyValue;
            return base.ContainsKey((TKey)key!);
        }

        public new bool Remove(TKey? key)
        {
            if (key is null)
            {
                if (_hasNullKeyValue)
                {
                    _hasNullKeyValue = false;
                    _nullKeyValue = default;
                    return true;
                }
                return false;
            }
            return base.Remove((TKey)key!);
        }

        public new void Clear()
        {
            base.Clear();
            _hasNullKeyValue = false;
            _nullKeyValue = default;
        }

        public new IEnumerator<KeyValuePair<TKey?, TValue>> GetEnumerator()
        {
            if (_hasNullKeyValue)
                yield return new KeyValuePair<TKey?, TValue>(default, _nullKeyValue!);
            // Iterate the underlying Dictionary entries
            foreach (KeyValuePair<TKey?, TValue> kv in (Dictionary<TKey?, TValue>)this)
            {
                yield return kv;
            }
        }
    }
}
