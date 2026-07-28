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

public static class DataOrErrorAsyncExtension
{
    public static async Task<TResult> ResolveAsync<T, TResult>(this Task<DataOrError<T>> value,
        Func<T, TResult> ifValid, Func<Exception, TResult> ifError)
    {
        var v = await value;
        return v.IsValid ? ifValid(v.Value) : ifError(v.Error);
    }

    public static async Task<DataOrError<TResult>> SelectAsync<T, TResult>(this Task<DataOrError<T>> value, Func<T, TResult> func)
    {
        var v = await value;

        return v.IsValid ? DataOrError.Try(() => func(v.Value)) : DataOrError.Error<TResult>(v.Error);
    }

    public static async Task<DataOrError<TResult>> SelectAsync<T, TResult>(this Task<DataOrError<T>> value, Func<T, Task<TResult>> func)
    {
        var v = await value;

        return v.IsValid ? await DataOrError.TryAsync(() => func(v.Value)) : DataOrError.Error<TResult>(v.Error);
    }

    public static async Task<DataOrError<TResult>> ThenAsync<T, TResult>(this Task<DataOrError<T>> value, Func<T, DataOrError<TResult>> func)
    {
        var v = await value;

        return v.IsValid
            ? DataOrError.Try(() => func(v.Value)).Flatten()
            : DataOrError.Error<TResult>(v.Error);
    }
    
    public static async Task<DataOrError<TResult>> ThenAsync<T, TResult>(this Task<DataOrError<T>> value, Func<T, Task<DataOrError<TResult>>> func)
    {
        var v = await value;

        return v.IsValid
            ? (await DataOrError.TryAsync(() => func(v.Value))).Flatten()
            : DataOrError.Error<TResult>(v.Error);
    }
}