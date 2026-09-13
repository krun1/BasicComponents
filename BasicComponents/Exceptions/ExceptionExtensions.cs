namespace BasicComponents.Exceptions;

public static class ExceptionExtensions
{
    public static IEnumerable<Exception> Extract(this AggregateException e)
    {
        foreach (var ex in e.InnerExceptions)
            if (ex is AggregateException ae)
                foreach (var inner in Extract(ae))
                    yield return inner;
            else
                yield return ex;
    }
}
