namespace Translation.Models;

public sealed record PhraseTranslation
{
    public required Phrase Source { get; init; }

    public required Phrase Target { get; init; }
}
