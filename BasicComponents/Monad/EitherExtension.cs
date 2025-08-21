namespace BasicComponents.Monad;

public static class EitherExtension
{
    public static TFirst AsFirst<TFirst, TSecond>(this Either<TFirst, TSecond> either, Func<TSecond, TFirst> selector)
        => either.IsFirst ? either.First : selector(either.Second);

    public static TSecond AsSecond<TFirst, TSecond>(this Either<TFirst, TSecond> either, Func<TFirst, TSecond> selector)
        => either.IsSecond ? either.Second : selector(either.First);
    
    public static TResult Resolve<TFirst, TSecond, TResult>(this Either<TFirst, TSecond> either,
        Func<TFirst, TResult> ifFirst,
        Func<TSecond, TResult> ifSecond)
        => either.IsFirst ? ifFirst(either.First) : ifSecond(either.Second);
}