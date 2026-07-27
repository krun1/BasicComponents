namespace BasicComponents.Monad;

public static class DataOrErrorExtension
{
    public static DataOrError<TResult> Then<T, TResult>(this DataOrError<T> value, Func<T, TResult> func)
        => value.IsValid ? DataOrError.Try(() => func(value.Value)) : DataOrError.Error<TResult>(value.Error);
}