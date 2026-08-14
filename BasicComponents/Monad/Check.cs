namespace BasicComponents.Monad;

public class Check
{
    private readonly Exception _error;

    private Check(Exception error, bool isValid)
    {
        _error = error;
        IsValid = isValid;
    }

    public bool IsValid { get; }
    public Exception Error => IsValid ? throw new InvalidOperationException($"No Error available") : _error;

    public static Check Success() => new(null!, true);
    public static Check Failure(Exception error) => new(error, false);
}

