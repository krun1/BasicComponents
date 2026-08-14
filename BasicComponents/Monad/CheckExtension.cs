namespace BasicComponents.Monad;

public static class CheckExtension
{
    public static DataOrError<T> Then<T>(this Check check, Func<T> value)
        => check.IsValid ? DataOrError.Create(value()) : DataOrError.Error<T>(check.Error);

    public static DataOrError<T> Then<T>(this Check check, Func<DataOrError<T>> value)
        => check.IsValid ? value() : DataOrError.Error<T>(check.Error);
}