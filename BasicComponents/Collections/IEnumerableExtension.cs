namespace BasicComponents.Collections;

public static class IEnumerableExtension
{
    public static bool RemoveIf<T>(this IList<T> collection, Func<T, bool> predicate)
    {
        var indexOf = collection.FirstIndexOf(predicate);
        
        if (indexOf == -1)
            return false;
        collection.RemoveAt(indexOf);
        return true;
    }

    public static int FirstIndexOf<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
    {
        var i = 0;

        foreach (var elem in collection)
        {
            if (predicate(elem))
                return i;
            ++i;
        }
        return -1;
    }

    public static bool AddUnique<T>(this IList<T> collection, T element)
    {
        if (collection.Contains(element))
            return false;
        collection.Add(element);
        return true;
    }
    
    
    public static async IAsyncEnumerable<T> WaitAll<T>(this IEnumerable<Task<T>> collection)
    {
        foreach (var task in collection)
            yield return await task;
    }
    
    public static IEnumerable<T> Randomize<T>(this IEnumerable<T> collection)
    {
        var random = new Random();

        return collection.OrderBy(x => random.Next());
    }

    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
            collection.Add(item);
    }

    public static void Deconstruct<T>(this IEnumerable<T> self, out T first, out T second)
    {
        if (!self.TryDeconstruct(out first, out second))
            throw new ArgumentException("The collection must contain exactly two elements.");
    }

    public static bool TryDeconstruct<T>(this IEnumerable<T> self, out T first, out T second)
    {
        var list = self.ToList();

        if (list.Count != 2)
        {
            first = default;
            second = default;
            return false;
        }
        first = list[0];
        second = list[1];
        return true;
    }

    public static bool TryGetElementAt<T>(this IEnumerable<T> self, int index, out T element)
    {
        int i = 0;
        
        foreach (var e in self)
        {
            if (i == index)
            {
                element = e;
                return true;
            }
            i++;
        }
        element = default!;
        return false;
    }
    
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

    public static void ForAll<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
            action(item);
    }
    
    public static T SingleOrThrow<T>(this IEnumerable<T> source, Func<Exception> ifEmpty, Func<Exception> ifMoreThanOne)
    {
        using var e = source.GetEnumerator();
        
        if (!e.MoveNext())
            throw ifEmpty();
        var current = e.Current;

        if (!e.MoveNext())
            throw ifMoreThanOne();
        return current;
    }
    
    public static T FirstOrThrow<T>(this IEnumerable<T> source, Func<Exception> ifEmpty)
    {
        using var e = source.GetEnumerator();
        
        if (!e.MoveNext())
            throw ifEmpty();
        return e.Current;
    }
    
    public static void AddMany<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(items);
        foreach (var item in items)
            collection.Add(item);
    }
}