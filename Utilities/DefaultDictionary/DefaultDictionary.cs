using System;
using System.Collections.Generic;

namespace Utilities.DefaultDictionary;

public class DefaultDictionary<TKey, TValue> : Dictionary<TKey, TValue>
{
    public Func<TValue> GetDefault { get; init; }

    public new TValue this[TKey key]
    {
        get
        {
            if (TryGetValue(key, out var value))
            {
                return value;
            }
            (this as Dictionary<TKey, TValue>)[key] = GetDefault();
            return (this as Dictionary<TKey, TValue>)[key];
        }
        set => (this as Dictionary<TKey, TValue>)[key] = value;
    }

    public DefaultDictionary()
    {
        GetDefault = () => default;
    }

    public DefaultDictionary(TValue defaultValue)
    {
        GetDefault = () => defaultValue;
    }

    public DefaultDictionary(Func<TValue> defaultFactory)
    {
        GetDefault = defaultFactory;
    }
}
