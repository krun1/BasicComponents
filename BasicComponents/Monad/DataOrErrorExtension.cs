namespace BasicComponents.Monad;

public static class DataOrErrorExtension
{
    public static DataOrError<T> Flatten<T>(this DataOrError<DataOrError<T>> value)
        => value.IsValid ? value.Value : DataOrError.Error<T>(value.Error);

    public static TResult Resolve<T, TResult>(this DataOrError<T> value, Func<T, TResult> ifValid, Func<Exception, TResult> ifError)
        => value.IsValid ? ifValid(value.Value) : ifError(value.Error);

    public static DataOrError<TResult> Select<T, TResult>(this DataOrError<T> value, Func<T, TResult> func)
        => value.IsValid ? DataOrError.Try(() => func(value.Value)) : DataOrError.Error<TResult>(value.Error);

    public static DataOrError<TResult> Then<T, TResult>(this DataOrError<T> value, Func<T, DataOrError<TResult>> func)
        => value.IsValid ? DataOrError.Try(() => func(value.Value)).Flatten() : DataOrError.Error<TResult>(value.Error);

    public static DataOrError<TValue> TryGetValue<TKey, TValue>(this DataOrError<TKey> key, Maybe.TryGetValueDelegate<TKey, TValue> func, Func<Either<string, Exception>> errorFunc)
        => key.Then(arg => DataOrError.TryGetValue(arg, func, errorFunc));

    public static DataOrError<TResult> Zip<TResult, T1, T2>(this DataOrError<T1> first, DataOrError<T2> second, Func<T1, T2, TResult> func)
    {
        return first.Resolve(l =>
        {
            return second.Resolve(r => DataOrError.Create(func(l, r)),
                DataOrError.Error<TResult>);
        }, e1 =>
        {
            return second.Resolve(r => DataOrError.Error<TResult>(e1),
                e2 => DataOrError.Error<TResult>(new AggregateException(e1, e2)));
        });
    }
}