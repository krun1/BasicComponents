namespace BasicComponents.Monad;

public class Either<TFirst, TSecond>
{
    private TFirst _first;
    private TSecond _second;
    private bool _isFirst;

    public bool IsFirst => _isFirst;
    public bool IsSecond => !_isFirst;
    
    public TFirst First => IsFirst ? _first : throw new InvalidOperationException("Either is not first");
    public TSecond Second => IsSecond ? _second : throw new InvalidOperationException("Either is not second");

    private Either(TFirst first, TSecond second, bool isFirst)
    {
        _first = first;
        _second = second;
        _isFirst = isFirst;
    }

    public static implicit operator Either<TFirst, TSecond>(TFirst value) => CreateFirst(value);

    public static implicit operator Either<TFirst, TSecond>(TSecond value) => CreateSecond(value);

    public static Either<TFirst, TSecond> CreateFirst(TFirst first) => new(first, default, true);
    public static Either<TFirst, TSecond> CreateSecond(TSecond second) => new(default, second, false);
}

public static class Either
{
    public static Either<TFirst, TSecond> Single<TFirst, TSecond>(TFirst? first, TSecond? second)
    {
        if (first != null)
            return Either<TFirst, TSecond>.CreateFirst(first);
        if (second != null)
            return Either<TFirst, TSecond>.CreateSecond(second);
        throw new InvalidOperationException("Impossible to create Either both value are null");
    }
}