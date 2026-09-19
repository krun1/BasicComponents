namespace BasicComponents.Monad;

public static class DataOrErrorExtension
{
    public static DataOrError<T> Flatten<T>(this DataOrError<DataOrError<T>> value)
        => value.IsValid ? value.Value : DataOrError.Error<T>(value.Failure);

    public static TResult Resolve<T, TResult>(this DataOrError<T> value, Func<T, TResult> ifValid, Func<BaseFailure, TResult> ifError)
        => value.IsValid ? ifValid(value.Value) : ifError(value.Failure);

    public static DataOrError<TResult> Select<T, TResult>(this DataOrError<T> value, Func<T, TResult> func)
        => value.IsValid ? DataOrError.Try(() => func(value.Value)) : DataOrError.Error<TResult>(value.Failure);

    public static DataOrError<TResult> Then<T, TResult>(this DataOrError<T> value, Func<T, DataOrError<TResult>> func)
        => value.IsValid ? DataOrError.Try(() => func(value.Value)).Flatten() : DataOrError.Error<TResult>(value.Failure);

    public static DataOrError<T> MapFailure<T, TError>(this DataOrError<T> value, Func<TError, BaseFailure> func)
        where TError : BaseFailure
        => value.IsValid ? value : DataOrError.Error<T>(value.Failure.Map(func));

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
                e2 => DataOrError.Error<TResult>(e1.Concat(e2)));
        });
    }

    public static DataOrError<T> Or<T>(this DataOrError<T> self, DataOrError<T> other) => self.IsValid ? self : other;
    public static DataOrError<T> Or<T>(this DataOrError<T> self, Func<DataOrError<T>> other) => self.IsValid ? self : other();

    public static T OrElse<T>(this DataOrError<T> self, T other) => self.IsValid ? self.Value : other;
    public static T OrElse<T>(this DataOrError<T> self, Func<T> other) => self.IsValid ? self.Value : other();

    public static Check Check<T>(this DataOrError<T> value, Func<T, Check> func)
        => value.Resolve(v => Monad.Check.Try(() => func(v)), Monad.Check.Fail);

    public static Check Execute<T>(this DataOrError<T> value, Action<T> func)
        => value.IsValid
            ? Monad.Check.Try(() => func(value.Value))
            : Monad.Check.Fail(value.Failure);
}


public static class DataOrErrorAsyncExtension
{
    public static async Task<TResult> ResolveAsync<T, TResult>(this Task<DataOrError<T>> value,
        Func<T, TResult> ifValid, Func<Exception, TResult> ifError)
    {
        var v = await value;
        return v.IsValid ? ifValid(v.Value) : ifError(v.Error);
    }

    public static async Task<DataOrError<T>> MapFailureAsync<T, TError>(this Task<DataOrError<T>> value,
        Func<TError, BaseFailure> func)
        where TError : BaseFailure
        => (await value).MapFailure(func);

    public static async Task<DataOrError<TResult>> SelectAsync<T, TResult>(this Task<DataOrError<T>> value, Func<T, TResult> func)
    {
        var v = await value;

        return v.IsValid ? DataOrError.Try(() => func(v.Value)) : DataOrError.Error<TResult>(v.Failure);
    }

    public static async Task<DataOrError<TResult>> SelectAsync<T, TResult>(this Task<DataOrError<T>> value, Func<T, Task<TResult>> func)
    {
        var v = await value;

        return v.IsValid ? await DataOrError.TryAsync(() => func(v.Value)) : DataOrError.Error<TResult>(v.Failure);
    }

    public static async Task<DataOrError<TResult>> SelectAsync<T, TResult>(this DataOrError<T> value, Func<T, Task<TResult>> func)
    {
        return value.IsValid
            ? await DataOrError.TryAsync(() => func(value.Value))
            : DataOrError.Error<TResult>(value.Failure);
    }

    public static async Task<DataOrError<TResult>> ThenAsync<T, TResult>(this Task<DataOrError<T>> value, Func<T, DataOrError<TResult>> func)
    {
        var v = await value;

        return v.IsValid
            ? DataOrError.Try(() => func(v.Value)).Flatten()
            : DataOrError.Error<TResult>(v.Failure);
    }

    public static async Task<DataOrError<TResult>> ThenAsync<T, TResult>(this Task<DataOrError<T>> value, Func<T, Task<DataOrError<TResult>>> func)
    {
        var v = await value;

        return v.IsValid
            ? (await DataOrError.TryAsync(() => func(v.Value))).Flatten()
            : DataOrError.Error<TResult>(v.Failure);
    }

    public static async Task<DataOrError<TResult>> ThenAsync<T, TResult>(this DataOrError<T> value, Func<T, Task<DataOrError<TResult>>> func)
    {
        return value.IsValid
            ? (await DataOrError.TryAsync(() => func(value.Value))).Flatten()
            : DataOrError.Error<TResult>(value.Failure);
    }

    public static async Task<DataOrError<T>> OrAsync<T>(this DataOrError<T> self, Func<Task<DataOrError<T>>> other)
        => self.IsValid ? self : await other();
    public static async Task<DataOrError<T>> OrAsync<T>(this Task<DataOrError<T>> self, Func<Task<DataOrError<T>>> other)
    {
        var res = await self;

        return res.IsValid ? res : await other();
    }

    public static async Task<T> OrElseAsync<T>(this DataOrError<T> self, Func<Task<T>> other)
        => self.IsValid ? self.Value : await other();

    public static async Task<T> OrElseAsync<T>(this Task<DataOrError<T>> self, Func<Task<T>> other)
    {
        var res = await self;

        return res.IsValid ? res.Value : await other();
    }
}