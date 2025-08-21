namespace BasicComponents.Monad;

public class DataOrError<T>(T value, string error, bool isValid)
{
    public bool IsValid { get; } = isValid;
    public T Value => IsValid ? value : throw new InvalidOperationException($"Value is not valid. Error : {error}");
    public string Error => IsValid ? throw new InvalidOperationException($"No Error available") : error;
    
    public static implicit operator DataOrError<T>(T value) => new(value, "", true);
}

public static class DataOrError
{
    public static DataOrError<T> Create<T>(T value) => new(value, "", true);
    
    public static DataOrError<T> Error<T>(string error) => new(default, error, false);
}