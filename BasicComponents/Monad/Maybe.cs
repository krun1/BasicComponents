namespace BasicComponents.Monad;

public readonly struct Maybe<T>
{
    private readonly T _value;

    internal Maybe(T value, bool isAvailable)
    {
        _value = value;
        IsAvailable = isAvailable;
    }

    public readonly T Value => IsAvailable ? _value : throw new InvalidOperationException("Value is missing");

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

public static class MaybeExtension
{
    public static TResult Resolve<T, TResult>(this Maybe<T> maybe, Func<T, TResult> ifAvailable, Func<TResult> ifMissing)
        => maybe.IsAvailable ? ifAvailable(maybe.Value) : ifMissing();
    
    public static Maybe<TResult> Select<T, TResult>(this Maybe<T> maybe, Func<T, TResult> func)
        => maybe.IsAvailable ? Maybe.Create(func(maybe.Value)) : Maybe.Missing<TResult>();
    
    public static Maybe<TResult> Then<T, TResult>(this Maybe<T> maybe, Func<T, Maybe<TResult>> func)
        => maybe.IsAvailable ? func(maybe.Value) : Maybe.Missing<TResult>();
    
    public static Maybe<T> Or<T>(this Maybe<T> maybe, Maybe<T> value)
        => maybe.IsAvailable ? maybe : value;
    
    public static Maybe<T> Or<T>(this Maybe<T> maybe, Func<Maybe<T>> value)
        => maybe.IsAvailable ? maybe : value();
    
    public static T OrElse<T>(this Maybe<T> maybe, T value)
        => maybe.IsAvailable ? maybe.Value : value;
    
    public static T Or<T>(this Maybe<T> maybe, Func<T> value)
        => maybe.IsAvailable ? maybe.Value : value();
    
    public static Maybe<T> Flatten<T>(this Maybe<Maybe<T>> maybe)
        => maybe.IsAvailable ? maybe.Value : Maybe.Missing<T>();
}