namespace BasicComponents.Monad;

public static class DataOrErrorStepExtension
{
    [Async]
    public static DataOrError<T>.Step<TResult> OnError<T, TError, TResult>(this DataOrError<T> self, Func<TError, TResult> func)
    where TError : BaseFailure
        => OnFailure(self, f => f.Cast<TError>(), func);

    [Async]
    public static DataOrError<T>.Step<TResult> OnError<T, TError, TResult>(this DataOrError<T>.Step<TResult> self,
        Func<TError, TResult> func)
        where TError : BaseFailure
        => self.OnError<T, TError, TResult>((e, r) => r.IsAvailable ? r.Value : func(e));

    [Async]
    public static DataOrError<T>.Step<TResult> OnError<T, TError, TResult>(this DataOrError<T>.Step<TResult> self,
        Func<TError, Maybe<TResult>, TResult> func)
        where TError : BaseFailure
        => OnFailure(self, f => f.Cast<TError>(), func);

    /// <summary>
    /// Like <c>OnError</c>, for the <see cref="ExceptionFailure"/> whose exception is a
    /// <typeparamref name="TException"/> or derives from it.
    /// </summary>
    [Async]
    public static DataOrError<T>.Step<TResult> OnException<T, TException, TResult>(this DataOrError<T> self,
        Func<TException, TResult> func)
        where TException : Exception
        => OnFailure(self, f => f.CastException<TException>(), f => func((TException)f.Exception));

    [Async]
    public static DataOrError<T>.Step<TResult> OnException<T, TException, TResult>(this DataOrError<T>.Step<TResult> self,
        Func<TException, TResult> func)
        where TException : Exception
        => self.OnException<T, TException, TResult>((e, r) => r.IsAvailable ? r.Value : func(e));

    [Async]
    public static DataOrError<T>.Step<TResult> OnException<T, TException, TResult>(this DataOrError<T>.Step<TResult> self,
        Func<TException, Maybe<TResult>, TResult> func)
        where TException : Exception
        => OnFailure(self, f => f.CastException<TException>(), (f, r) => func((TException)f.Exception, r));

    private static DataOrError<T>.Step<TResult> OnFailure<T, TError, TResult>(DataOrError<T> self,
        Func<BaseFailure, TError?> select, Func<TError, TResult> func)
        where TError : BaseFailure
    {
        if (self.IsValid)
            return new DataOrError<T>.Step<TResult>(self, default!);
        try
        {
            var failure = select(self.Failure);

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

    private static DataOrError<T>.Step<TResult> OnFailure<T, TError, TResult>(DataOrError<T>.Step<TResult> self,
        Func<BaseFailure, TError?> select, Func<TError, Maybe<TResult>, TResult> func)
        where TError : BaseFailure
    {
        if (self.Inner.IsValid)
            return self;
        try
        {
            var failure = select(self.Inner.Failure);

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

    [Async]
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

    [Async]
    public static Check Check<T>(this DataOrError<T>.Step<T> self, Func<T, Check> check)
    {
        if (self.Inner.IsValid)
            return check(self.Inner.Value);
        return self.Inner.Failure.IsHandled
            ? check(self.Result.Value)
            : Monad.Check.Fail(self.Inner.Failure);
    }
}

public static class DataOrErrorStepAsyncExtension
{
    [Async]
    public static Task<DataOrError<T>.Step<TResult>> OnErrorAsync<T, TError, TResult>(this DataOrError<T> self,
        Func<TError, Task<TResult>> func) where TError : BaseFailure
        => OnFailureAsync(self, f => f.Cast<TError>(), func);

    [Async]
    public static Task<DataOrError<T>.Step<TResult>> OnErrorAsync<T, TError, TResult>(this DataOrError<T>.Step<TResult> self,
        Func<TError, Task<TResult>> func) where TError : BaseFailure
        => self.OnErrorAsync<T, TError, TResult>((error, maybe) => func(error));

    [Async]
    public static Task<DataOrError<T>.Step<TResult>> OnErrorAsync<T, TError, TResult>(this DataOrError<T>.Step<TResult> self,
        Func<TError, Maybe<TResult>, Task<TResult>> func) where TError : BaseFailure
        => OnFailureAsync(self, f => f.Cast<TError>(), func);

    [Async]
    public static Task<DataOrError<T>.Step<TResult>> OnExceptionAsync<T, TException, TResult>(this DataOrError<T> self,
        Func<TException, Task<TResult>> func) where TException : Exception
        => OnFailureAsync(self, f => f.CastException<TException>(), f => func((TException)f.Exception));

    [Async]
    public static Task<DataOrError<T>.Step<TResult>> OnExceptionAsync<T, TException, TResult>(this DataOrError<T>.Step<TResult> self,
        Func<TException, Task<TResult>> func) where TException : Exception
        => self.OnExceptionAsync<T, TException, TResult>((error, maybe) => func(error));

    [Async]
    public static Task<DataOrError<T>.Step<TResult>> OnExceptionAsync<T, TException, TResult>(this DataOrError<T>.Step<TResult> self,
        Func<TException, Maybe<TResult>, Task<TResult>> func) where TException : Exception
        => OnFailureAsync(self, f => f.CastException<TException>(), (f, r) => func((TException)f.Exception, r));

    private static async Task<DataOrError<T>.Step<TResult>> OnFailureAsync<T, TError, TResult>(DataOrError<T> self,
        Func<BaseFailure, TError?> select, Func<TError, Task<TResult>> func) where TError : BaseFailure
    {
        if (self.IsValid)
            return new DataOrError<T>.Step<TResult>(self, default!);
        try
        {
            var failure = select(self.Failure);

            if (failure != null)
            {
                var result = await func(failure);
                return new DataOrError<T>.Step<TResult>(DataOrError.Error<T>(self.Failure.Handle(failure)), Maybe.Create(result));
            }
            return new DataOrError<T>.Step<TResult>(self, default!);
        }
        catch (Exception e)
        {
            return DataOrError.Error<T>(e).AsStep(Maybe.Missing<TResult>());
        }
    }

    private static async Task<DataOrError<T>.Step<TResult>> OnFailureAsync<T, TError, TResult>(DataOrError<T>.Step<TResult> self,
        Func<BaseFailure, TError?> select, Func<TError, Maybe<TResult>, Task<TResult>> func) where TError : BaseFailure
    {
        if (self.Inner.IsValid)
            return self;
        try
        {
            var failure = select(self.Inner.Failure);

            if (failure != null)
            {
                var result = await func(failure, self.Result);
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

    [Async]
    public static async Task<Check> CheckAsync<T>(this DataOrError<T>.Step<T> self, Func<T, Task<Check>> check)
    {
        if (self.Inner.IsValid)
            return await check(self.Inner.Value);
        return self.Inner.Failure.IsHandled
            ? await check(self.Result.Value)
            : Monad.Check.Fail(self.Inner.Failure);
    }

    [Async]
    public static async Task<TResult> ResolveAsync<T, TResult>(this DataOrError<T>.Step<TResult> self,
        Func<T, Task<TResult>> ifSuccess, Func<BaseFailure, Task<TResult>>? ifFailure = null)
    {
        if (self.Inner.IsValid)
            return await ifSuccess(self.Inner.Value);
        if (self.Inner.Failure.IsHandled)
            return self.Result.Value;
        if (ifFailure != null)
            return await ifFailure(self.Inner.Failure);
        throw new InvalidOperationException("Failure not handled", self.Inner.Failure.ToException());
    }
}
