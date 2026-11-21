namespace Translation.Models.Comparers;

public static class EqualityComparers
{
    public static readonly StringComparer LanguageCodeComparer = StringComparer.OrdinalIgnoreCase;

    public static readonly StringComparer TextComparer = StringComparer.OrdinalIgnoreCase;
}
