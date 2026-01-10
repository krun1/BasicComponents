using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace BasicComponents.Collections;

public class LazyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _dictionary;
    private readonly Func<TKey, TValue> _keySelector;

    public LazyDictionary(Func<TKey, TValue> keySelector, IEqualityComparer<TKey>? comparer)
    {
        _dictionary =  new Dictionary<TKey, TValue>(comparer);
        _keySelector = keySelector;
    }

    public LazyDictionary(Func<TKey, TValue> keySelector)
    {
        _dictionary =  new Dictionary<TKey, TValue>();
        _keySelector = keySelector;
    }

    public TValue this[TKey key]
    {
        get
        {
            if (!_dictionary.TryGetValue(key, out var value))
                value = _dictionary[key] = _keySelector(key);
            return value;
        }
    }


    #region IReadOnlyDictionary

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_dictionary).GetEnumerator();
    }

    public int Count => _dictionary.Count;

    public bool ContainsKey(TKey key)
    {
        return _dictionary.ContainsKey(key);
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _dictionary.TryGetValue(key, out value);
    }
    
    public IEnumerable<TKey> Keys => _dictionary.Keys;

    public IEnumerable<TValue> Values => _dictionary.Values;

    #endregion
}