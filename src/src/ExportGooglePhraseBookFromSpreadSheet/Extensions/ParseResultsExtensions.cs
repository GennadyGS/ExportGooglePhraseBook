using System.Diagnostics;
using ExportGooglePhraseBookFromSpreadSheet.Models;

namespace ExportGooglePhraseBookFromSpreadSheet.Extensions;

internal static class ParseResultsExtensions
{
    public static (IReadOnlyCollection<T> successes, IReadOnlyCollection<string> errors) Split<T>(
        this IEnumerable<ParseResult<T>> source) =>
        source
            .Aggregate(
                (successes: Enumerable.Empty<T>(), errors: Enumerable.Empty<string>()),
                (acc, r) => r switch
                {
                    ParseResult<T>.Success success =>
                        (acc.successes.Append<T>(success.Result), acc.errors),
                    ParseResult<T>.Error error =>
                        (acc.successes, acc.errors.Append(error.Message)),
                    _ => throw new UnreachableException("Impossible case."),
                })
            .BiMap(successes => successes.ToList(), errors => errors.ToList());
}
