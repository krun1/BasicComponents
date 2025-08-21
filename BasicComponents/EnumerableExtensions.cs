namespace BasicComponents;

public static class EnumerableExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) => source.Where(x => x != null)!;
    
    public static void SmartForeach<T>(this IEnumerable<T> source,
        Action<T>? first = null,
        Action<T>? each = null,
        Action<T>? last = null)
    {
        ArgumentNullException.ThrowIfNull(each);
        using var e = source.GetEnumerator();

        if (!e.MoveNext() && (first != null || last != null))
            throw new InvalidOperationException("Sequence contains no elements");
        if (first != null)
        {
            first(e.Current);
            if (!e.MoveNext() && last != null)
                throw new InvalidOperationException("Sequence do not contain enough elements to call first and last");
        }
        var c = e.Current;

        while (e.MoveNext())
        {
            each.Invoke(c);
            c = e.Current;
        }

        if (last != null)
            last(c);
        else
            each.Invoke(c);
    }
}