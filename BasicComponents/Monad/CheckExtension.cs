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

    [Async]
    public static Check OnError<TError>(this Check check, Action<TError> func) where TError : BaseFailure
    {
        if (!check.IsValid)
        {
            var failure = check.Failure.Cast<TError>();

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