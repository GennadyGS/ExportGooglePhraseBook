namespace Translation.Models;

public sealed record Phrase
{
    public required string Text { get; init; }

    public required string LanguageCode { get; init; }
}
