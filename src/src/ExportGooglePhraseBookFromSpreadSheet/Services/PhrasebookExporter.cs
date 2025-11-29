using System.Globalization;
using ExportGooglePhraseBookFromSpreadSheet.Extensions;
using ExportGooglePhraseBookFromSpreadSheet.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Translation.Models;

namespace ExportGooglePhraseBookFromSpreadSheet.Services;

public static class PhrasebookExporter
{
    private const string CredentialFileName = "credential.json";
    private const string PhrasebookRange = "A:D";

    public static void Export(string spreadsheetId, string outputFilePath)
    {
        var phraseTranslations = LoadPhraseTranslations(spreadsheetId);
        SavePhraseTranslationsToFile(phraseTranslations, outputFilePath);
    }

    private static IReadOnlyCollection<PhraseTranslation> LoadPhraseTranslations(string spreadsheetId)
    {
        var service = CreateSheetsService();
        var request = service.Spreadsheets.Values.Get(spreadsheetId, PhrasebookRange);
        var response = request.Execute();
        var languageMap = GetLanguageMap();
        var (phraseTranslations, errors) = response.Values
            .Select(values => ParsePhraseTranslation(values, languageMap))
            .Split();
        var logger = CreateLogger();
        foreach (var errorMessage in errors)
        {
            logger.LogWarning(errorMessage);
        }

        return phraseTranslations;
    }

    private static IReadOnlyDictionary<string, string> GetLanguageMap() =>
        CultureInfo.GetCultures(CultureTypes.NeutralCultures)
            .ToDictionary(
                c => c.EnglishName,
                c => c.TwoLetterISOLanguageName,
                StringComparer.OrdinalIgnoreCase);

    private static ParseResult<PhraseTranslation> ParsePhraseTranslation(
        IList<object> values, IReadOnlyDictionary<string, string> languageMap)
    {
        if (values is not
            [string sourceLang, string targetLang, string sourceText, string targetText])
        {
            return new ParseResult<PhraseTranslation>.Error("Unrecognized format of phrase entry");
        }

        if (!languageMap.TryGetValue(sourceLang, out var sourceLangCode))
        {
            return new ParseResult<PhraseTranslation>.Error($"Unsupported language: {sourceLang}");
        }

        if (!languageMap.TryGetValue(targetLang, out var targetLangCode))
        {
            return new ParseResult<PhraseTranslation>.Error($"Unsupported language: {targetLangCode}");
        }

        return new ParseResult<PhraseTranslation>.Success(
            new()
            {
                Source = new()
                {
                    Text = sourceText,
                    LanguageCode = sourceLangCode,
                },
                Target = new()
                {
                    Text = targetText,
                    LanguageCode = targetLangCode,
                },
            });
    }

    private static ILogger CreateLogger() =>
        LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger(typeof(PhrasebookExporter).Namespace!);

    private static void SavePhraseTranslationsToFile(
        IReadOnlyCollection<PhraseTranslation> phraseTranslations, string outputFilePath)
    {
        var content = JsonConvert.SerializeObject(phraseTranslations, Formatting.Indented);
        File.WriteAllText(outputFilePath, content);
    }

    private static SheetsService CreateSheetsService()
    {
        var initializer = new BaseClientService.Initializer
        {
            HttpClientInitializer = LoadCredential(),
        };
        return new(initializer);
    }

    private static GoogleCredential LoadCredential()
    {
        var scope = SheetsService.Scope.SpreadsheetsReadonly;
        var credentialFullFilePath = Path.GetFullPath(CredentialFileName, AppContext.BaseDirectory);
        using var stream = new FileStream(credentialFullFilePath, FileMode.Open, FileAccess.Read);
        return CredentialFactory.FromStream<ServiceAccountCredential>(stream)
            .ToGoogleCredential()
            .CreateScoped(scope);
    }
}
