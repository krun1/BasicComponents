namespace BasicComponents.Monad;

public readonly struct Maybe<T>
{
    private readonly T _value;

    internal Maybe(T value, bool isAvailable)
    {
        _value = value;
        IsAvailable = isAvailable;
    }

    public T Value => IsAvailable ? _value : throw new InvalidOperationException("Value is missing");

    public bool IsAvailable { get; }
}

public static class Maybe
{
    public static Maybe<T> Create<T>(T value) => new(value, true);
    public static Maybe<T> Missing<T>() => new(default!, false);

    public static Maybe<T> IfNotNull<T>(T? v) where T : class
        => v != null ? Create(v) : Missing<T>();

    public delegate bool TryGetValueDelegate<TKey, TValue>(TKey key, out TValue value);
    public static Maybe<TValue> TryGetValue<TKey, TValue>(TKey key, TryGetValueDelegate<TKey, TValue> func)
        => func(key, out var value) ? Create(value) : Missing<TValue>();
}