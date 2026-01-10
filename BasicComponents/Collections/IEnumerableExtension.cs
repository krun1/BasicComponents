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
}