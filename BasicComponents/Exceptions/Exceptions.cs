namespace BasicComponents.Exceptions;

public static class Exceptions
{
    public static IEnumerable<Exception> Extract(Exception e)
    {
        if (e is AggregateException ae)
            return ae.Extract();
        return [e];
    }
}
