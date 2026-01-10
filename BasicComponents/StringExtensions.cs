namespace BasicComponents;

public static class StringExtensions
{
    public static string RemoveStart(this string str, string prefix, StringComparison comparer)
        => str.StartsWith(prefix, comparer) ? str[prefix.Length..] : str;

    public static string RemoveStart(this string str, string prefix)
        => RemoveStart(str, prefix, StringComparison.Ordinal);
}