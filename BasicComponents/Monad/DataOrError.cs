namespace BasicComponents.Monad;

public class DataOrError<T>(T value, Exception error, bool isValid)
{
    public bool IsValid { get; } = isValid;
    public T Value => IsValid ? value : throw new InvalidOperationException($"Value is not valid.", error);
    public Exception Error => IsValid ? throw new InvalidOperationException($"No Error available") : error;
    
    public static implicit operator DataOrError<T>(T value) => new(value, null!, true);
}

public static class DataOrError
{
    public static DataOrError<T> Create<T>(T value) => new(value, null!, true);
    public static DataOrError<T> Error<T>(Exception error) => new(default!, error, false);
    public static DataOrError<T> Error<T>(string error) => new(default!, new Exception(error), false);

    public static DataOrError<T> Try<T>(Func<T> func)
    {
        try
        {
            return func();
        }
        catch (Exception e)
        {
            return Error<T>(e);
        }
    }
    
    public static DataOrError<TValue> TryGetValue<TKey, TValue>(TKey key, Maybe.TryGetValueDelegate<TKey, TValue> func, Func<Either<string, Exception>> errorFunc)
        => func(key, out var value) ? Create(value) : errorFunc().Resolve(Error<TValue>, Error<TValue>);
    
    public static async Task<DataOrError<T>> TryAsync<T>(Func<Task<T>> func)
    {
        try
        {
            return await func();
        }
        catch (Exception e)
        {
            return Error<T>(e);
        }
    }

}