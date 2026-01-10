using System;
using System.Text;
using Microsoft.CodeAnalysis;

namespace ClassWrapperGenerator;

internal static class StringExtensions
{
    public static string RemoveStart(this string str, string prefix, StringComparison comparer)
        => str.StartsWith(prefix, comparer) ? str[prefix.Length..] : str;

    public static string RemoveStart(this string str, string prefix)
        => RemoveStart(str, prefix, StringComparison.Ordinal);

    public static string FullName(this ISymbol symbol)
    {
        if (symbol == null)
            return "";
        var sb = new StringBuilder();

        if (symbol.ContainingNamespace is {} namespaceSymbol)
            sb.Append(namespaceSymbol.ToDisplayString());

        if (symbol.ContainingType is {} typeSymbol)
        {
            sb.Append('.');
            sb.Append(typeSymbol.Name);
        }
        sb.Append('.');
        sb.Append(symbol.Name);
        return sb.ToString();
    }
}