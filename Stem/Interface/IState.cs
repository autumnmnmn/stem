namespace Stem;

public interface IState
{
    void Set<T>(T value);

    void Set<T>(string key, T value);

    void Set<TKey, TValue>(TKey key, TValue value);

    T Get<T>();

    T Get<T>(string key);

    TValue Get<TKey, TValue>(TKey key);

    bool IsSet<T>();

    bool IsSet<T>(string key);

    bool IsSet<TKey, TValue>(TKey key);

    void Clear<T>();

    void Clear<TKey, TValue>();
}
