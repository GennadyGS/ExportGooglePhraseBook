namespace Common.Extensions;

public static class StringsExtensions
{
    public static string JoinStrings<T>(this IEnumerable<T> strings, string separator) =>
        string.Join(separator, strings);
}
