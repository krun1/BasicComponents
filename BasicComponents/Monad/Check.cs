namespace BasicComponents.Monad;

public class Check
{
    private readonly BaseFailure _error;

    private Check(BaseFailure error, bool isValid)
    {
        _error = error;
        IsValid = isValid;
    }

    public bool IsValid { get; }
    public Exception Error => IsValid ? throw new InvalidOperationException("No Error available") : _error.ToException();
    public BaseFailure Failure => IsValid ? throw new InvalidOperationException("No Failure available") : _error;
    
    public static Check operator &(Check left, Check right)
    {
        return left.IsValid && right.IsValid
            ? Success()
            : Fail(new AggregateException(left.Error, right.Error).Flatten());
    }

    public static Check operator |(Check left, Check right)
    {
        return left.IsValid || right.IsValid
            ? Success()
            : Fail(new AggregateException(left.Error, right.Error).Flatten());
    }
    
    
    public static Check Success() => new(null!, true);
    public static Check Fail(Exception error) => new(new ExceptionFailure(error), false);
    public static Check Fail(BaseFailure error) => new(error, false);
    public static Check Try(Action action)
    {
        try
        {
            action();
            return Success();
        }
        catch (Exception e)
        {
            return Fail(e);
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
            return Fail(e);
        }
    }
}

