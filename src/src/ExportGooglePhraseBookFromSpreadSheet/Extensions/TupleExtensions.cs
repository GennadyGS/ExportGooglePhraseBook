namespace ExportGooglePhraseBookFromSpreadSheet.Extensions;

public static class TupleExtensions
{
    public static (TResult fst, TResult snd) Map<TSource, TResult>(
        this (TSource fst, TSource snd) source, Func<TSource, TResult> mapper) =>
        (mapper(source.fst), mapper(source.snd));

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S2436:Types and methods should not have too many generic parameters",
        Justification = "Exceptional case")]
    public static (TResult1 fst, TResult2 snd) BiMap<TSource1, TSource2, TResult1, TResult2>(
        this (TSource1 fst, TSource2 snd) source,
        Func<TSource1, TResult1> mapper1,
        Func<TSource2, TResult2> mapper2) =>
        (mapper1(source.fst), mapper2(source.snd));
}
