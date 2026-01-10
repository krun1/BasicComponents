namespace BasicComponents.Collections;

public static class IAsyncEnumerableExtension
{
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> collection)
    {
        var list = new List<T>();

        await foreach (var elem in collection)
            list.Add(elem);
        return list;
    }
    
    public static async IAsyncEnumerable<T> ToListAsync<T>(this Task<List<T>> collection)
    {
        var c = await collection;

        foreach (var x1 in c)
            yield return x1;
    }
}