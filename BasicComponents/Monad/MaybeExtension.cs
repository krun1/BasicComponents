namespace BasicComponents.Monad;

public static class MaybeExtension
{
    public static TResult Resolve<T, TResult>(this Maybe<T> maybe, Func<T, TResult> ifAvailable, Func<TResult> ifMissing)
        => maybe.IsAvailable ? ifAvailable(maybe.Value) : ifMissing();
    
    public static Maybe<TResult> Select<T, TResult>(this Maybe<T> maybe, Func<T, TResult> func)
        => maybe.IsAvailable ? Maybe.Create(func(maybe.Value)) : Maybe.Missing<TResult>();
    
    public static Maybe<TResult> Then<T, TResult>(this Maybe<T> maybe, Func<T, Maybe<TResult>> func)
        => maybe.IsAvailable ? func(maybe.Value) : Maybe.Missing<TResult>();
    
    public static Maybe<T> Or<T>(this Maybe<T> maybe, Maybe<T> value)
        => maybe.IsAvailable ? maybe : value;
    
    public static Maybe<T> Or<T>(this Maybe<T> maybe, Func<Maybe<T>> value)
        => maybe.IsAvailable ? maybe : value();
    
    public static T OrElse<T>(this Maybe<T> maybe, T value)
        => maybe.IsAvailable ? maybe.Value : value;
    
    public static T Or<T>(this Maybe<T> maybe, Func<T> value)
        => maybe.IsAvailable ? maybe.Value : value();
    
    public static Maybe<T> Flatten<T>(this Maybe<Maybe<T>> maybe)
        => maybe.IsAvailable ? maybe.Value : Maybe.Missing<T>();
    
    public static DataOrError<T> ToDataOrError<T>(this Maybe<T> maybe, Func<Exception> errorFunc)
        => maybe.IsAvailable ? DataOrError.Create(maybe.Value) : DataOrError.Error<T>(errorFunc());
    
    public static DataOrError<T> ToDataOrError<T>(this Maybe<T> maybe, Func<string> errorFunc)
        => maybe.IsAvailable ? DataOrError.Create(maybe.Value) : DataOrError.Error<T>(errorFunc());

}