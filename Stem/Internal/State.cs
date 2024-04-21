using Utilities.DefaultDictionary;
using System.Collections.Generic;

namespace Stem.Internal;

internal class State : IState
{
    private static int __nextId = 0;
    private readonly int __id;

    public State()
    {
        __id = __nextId;
        __nextId += 1;
    }

    public void Set<T>(T value)
    {
        TypedState<T>.ValueById[__id] = value;
    }

    public void Set<T>(string key, T value) => Set<string, T>(key, value);

    public void Set<TKey, TValue>(TKey key, TValue value)
    {
        TypedState<TKey, TValue>.ValueByKeyById[__id][key] = value;
    }

    public T Get<T>() => TypedState<T>.ValueById[__id];

    public T Get<T>(string key) => Get<string, T>(key);

    public TValue Get<TKey, TValue>(TKey key) => TypedState<TKey, TValue>.ValueByKeyById[__id][key];

    public bool IsSet<T>() => TypedState<T>.ValueById.ContainsKey(__id);

    public bool IsSet<T>(string key) => IsSet<string, T>(key);

    public bool IsSet<TKey, TValue>(TKey key) => TypedState<TKey, TValue>.ValueByKeyById[__id].ContainsKey(key);

    public void Clear<T>()
    {
        TypedState<T>.ValueById.Remove(__id);
        Clear<string, T>();
    }

    public void Clear<TKey, TValue>() => TypedState<TKey, TValue>.ValueByKeyById.Remove(__id);

    private static class TypedState<T>
    {
        public static Dictionary<int, T> ValueById { get; } = new();
    }

    private static class TypedState<TKey, TValue>
    {
        public static DefaultDictionary<int, Dictionary<TKey, TValue>> ValueByKeyById { get; } = new(() => new());
    }
}
