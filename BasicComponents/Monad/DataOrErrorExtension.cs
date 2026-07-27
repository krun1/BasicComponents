namespace BasicComponents.Monad;

public static class DataOrErrorExtension
{
    public static DataOrError<T> Flatten<T>(this DataOrError<DataOrError<T>> value)
        => value.IsValid ? value.Value : DataOrError.Error<T>(value.Error);

    public static DataOrError<TResult> Select<T, TResult>(this DataOrError<T> value, Func<T, TResult> func)
        => value.IsValid ? DataOrError.Try(() => func(value.Value)) : DataOrError.Error<TResult>(value.Error);

    public static DataOrError<TResult> Then<T, TResult>(this DataOrError<T> value, Func<T, DataOrError<TResult>> func)
        => value.IsValid ? DataOrError.Try(() => func(value.Value)).Flatten() : DataOrError.Error<TResult>(value.Error);

    public static DataOrError<TValue> TryGetValue<TKey, TValue>(this DataOrError<TKey> key, Maybe.TryGetValueDelegate<TKey, TValue> func, Func<Either<string, Exception>> errorFunc)
        => key.Then(arg => DataOrError.TryGetValue(arg, func, errorFunc));
}