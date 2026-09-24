namespace BasicComponents.Monad;

public static class CheckExtension
{
    [Async]
    public static DataOrError<T> Select<T>(this Check check, Func<T> value)
        => check.IsValid ? DataOrError.Create(value()) : DataOrError.Error<T>(check.Failure);

    [Async]
    public static DataOrError<T> Then<T>(this Check check, Func<DataOrError<T>> value)
        => check.IsValid ? value() : DataOrError.Error<T>(check.Failure);
    
    [Async]
    public static Check Then(this Check check, Func<Check> value)
        => check.IsValid ? Check.Try(value) : check;

    [Async]
    public static Check And(this Check check, Check other)
        => check.IsValid && other.IsValid 
            ? Check.Success()
            : Check.Fail(new AggregateException(check.Error, other.Error).Flatten());

    [Async]
    public static Check MapFailure<TError>(this Check check, Func<TError, BaseFailure> func) where TError : BaseFailure
        => check.IsValid ? check : Check.Fail(check.Failure.Map(func));

    /// <summary>
    /// Like <see cref="MapFailure{TError}"/>, for the <see cref="ExceptionFailure"/> whose exception is a
    /// <typeparamref name="TException"/> or derives from it. Once mapped, a failure is no longer an
    /// <see cref="ExceptionFailure"/>, so chained calls map each exception at most once.
    /// </summary>
    [Async]
    public static Check MapException<TException>(this Check check, Func<TException, BaseFailure> func) where TException : Exception
        => check.MapFailure((ExceptionFailure f) => f.Exception is TException e ? func(e) : f);

    [Async]
    public static Check OnError<TError>(this Check check, Action<TError> func) where TError : BaseFailure
        => OnFailure(check, f => f.Cast<TError>(), func);

    /// <summary>
    /// Like <see cref="OnError{TError}"/>, for the <see cref="ExceptionFailure"/> whose exception is a
    /// <typeparamref name="TException"/> or derives from it.
    /// </summary>
    [Async]
    public static Check OnException<TException>(this Check check, Action<TException> func) where TException : Exception
        => OnFailure(check, f => f.CastException<TException>(), f => func((TException)f.Exception));

    private static Check OnFailure<TError>(Check check, Func<BaseFailure, TError?> select, Action<TError> func)
        where TError : BaseFailure
    {
        if (!check.IsValid)
        {
            var failure = select(check.Failure);

            if (failure != null)
            {
                try
                {
                    func(failure);
                }
                catch (Exception e)
                {
                    return check & Check.Fail(e);
                }
                return Check.Fail(check.Failure.Handle(failure));
            }
        }
        return check;
    }
}

public static class CheckAsyncExtension
{
    [Async]
    public static async Task<DataOrError<T>> SelectAsync<T>(this Check check, Func<Task<T>> value)
        => check.IsValid ? DataOrError.Create(await value()) : DataOrError.Error<T>(check.Failure);

    [Async]
    public static async Task<DataOrError<T>> ThenAsync<T>(this Check check, Func<Task<DataOrError<T>>> value)
    {
        return check.IsValid ? await value() : DataOrError.Error<T>(check.Failure);
    }
}