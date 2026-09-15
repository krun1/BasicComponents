namespace BasicComponents.Monad;

public static class CheckExtension
{
    public static DataOrError<T> Select<T>(this Check check, Func<T> value)
        => check.IsValid ? DataOrError.Create(value()) : DataOrError.Error<T>(check.Failure);

    public static DataOrError<T> Then<T>(this Check check, Func<DataOrError<T>> value)
        => check.IsValid ? value() : DataOrError.Error<T>(check.Failure);
    
    public static Check Then(this Check check, Func<Check> value)
        => check.IsValid ? Check.Try(value) : check;

    public static Check And(this Check check, Check other)
        => check.IsValid && other.IsValid 
            ? Check.Success()
            : Check.Fail(new AggregateException(check.Error, other.Error).Flatten());
}

public static class CheckAsyncExtension
{
    public static async Task<DataOrError<T>> SelectAsync<T>(this Check check, Func<Task<T>> value)
        => check.IsValid ? DataOrError.Create(await value()) : DataOrError.Error<T>(check.Failure);

    public static async Task<DataOrError<T>> ThenAsync<T>(this Check check, Func<Task<DataOrError<T>>> value)
    {
        return check.IsValid ? await value() : DataOrError.Error<T>(check.Failure);
    }
}