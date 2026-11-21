namespace ExportGooglePhraseBookFromSpreadSheet.Models;

internal record ParseResult<T>
{
    public sealed record Success(T Result) : ParseResult<T>;

    public sealed record Error(string Message) : ParseResult<T>;
}
