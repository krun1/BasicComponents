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
    public static Check Try(Action action)
    {
        try
        {
            action();
            return Success();
        }
        catch (Exception e)
        {
            return Failure(e);
        }
    }

    public static Check Try(Func<Check> action)
    {
        try
        {
            return action();
        }
        catch (Exception e)
        {
            return Failure(e);
        }
    }
}

