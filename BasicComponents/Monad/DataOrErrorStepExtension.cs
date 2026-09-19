namespace BasicComponents.Monad;

public static class DataOrErrorStepExtension
{
    public static DataOrError<T>.Step<TResult> OnError<T, TError, TResult>(this DataOrError<T> self, Func<TError, TResult> func)
    where TError : BaseFailure
    {
        if (self.IsValid)
            return new DataOrError<T>.Step<TResult>(self, default!);
        try
        {
            var failure = self.Failure.Cast<TError>();

            if (failure != null)
            {
                var result = func(failure);
                return new DataOrError<T>.Step<TResult>(DataOrError.Error<T>(self.Failure.Handle(failure)), Maybe.Create(result));
            }
            return new DataOrError<T>.Step<TResult>(self, default!);
        }
        catch (Exception e)
        {
            return DataOrError.Error<T>(e).AsStep(Maybe.Missing<TResult>());
        }
    }

    public static DataOrError<T>.Step<TResult> OnError<T, TError, TResult>(this DataOrError<T>.Step<TResult> self,
        Func<TError, TResult> func)
        where TError : BaseFailure
        => self.OnError<T, TError, TResult>((e, r) => r.IsAvailable ? r.Value : func(e));

    public static DataOrError<T>.Step<TResult> OnError<T, TError, TResult>(this DataOrError<T>.Step<TResult> self,
        Func<TError, Maybe<TResult>, TResult> func)
        where TError : BaseFailure
    {
        if (self.Inner.IsValid)
            return self;
        try
        {
            var failure = self.Inner.Failure.Cast<TError>();

            if (failure != null)
            {
                var result = func(failure, self.Result);
                return new DataOrError<T>.Step<TResult>(
                    DataOrError.Error<T>(self.Inner.Failure.Handle(failure)),
                    Maybe.Create(result));
            }
            return self;
        }
        catch (Exception e)
        {
            return DataOrError.Error<T>(e).AsStep(Maybe.Missing<TResult>());
        }
    }
    
    public static TResult Resolve<T, TResult>(this DataOrError<T>.Step<TResult> self,
        Func<T, TResult> ifSuccess, Func<BaseFailure, TResult>? ifFailure = null)
    {
        if (self.Inner.IsValid)
            return ifSuccess(self.Inner.Value);
        if (self.Inner.Failure.IsHandled)
            return self.Result.Value;
        if (ifFailure != null)
            return ifFailure(self.Inner.Failure);
        throw new InvalidOperationException("Failure not handled", self.Inner.Failure.ToException());
    }

    public static Check Check<T>(this DataOrError<T>.Step<T> self, Func<T, Check> check)
    {
        if (self.Inner.IsValid)
            return check(self.Inner.Value);
        return self.Inner.Failure.IsHandled
            ? check(self.Result.Value)
            : Monad.Check.Fail(self.Inner.Failure);
    }
    
    public static async Task<Check> CheckAsync<T>(this DataOrError<T>.Step<T> self, Func<T, Task<Check>> check)
    {
        if (self.Inner.IsValid)
            return await check(self.Inner.Value);
        return self.Inner.Failure.IsHandled
            ? await check(self.Result.Value)
            : Monad.Check.Fail(self.Inner.Failure);
    }
    
    public static async Task<Check> CheckAsync<T>(this Task<DataOrError<T>.Step<T>> self, Func<T, Task<Check>> check)
        => await (await self).CheckAsync(async arg => await check(arg));
}