using System.Globalization;
using CommandLine;
using ExportGooglePhraseBookFromSpreadSheet.Configuration;
using ExportGooglePhraseBookFromSpreadSheet.Extensions;
using ExportGooglePhraseBookFromSpreadSheet.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Translation.Models;

namespace ExportGooglePhraseBookFromSpreadSheet;

public static class Program
{
    private const string CredentialFileName = "credential.json";
    private const string PhrasebookRange = "A:D";
    private const int ErrorExitCode = 2;

    public static int Main(string[] args)
    {
        try
        {
            return RunCommandLineParser(args);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return ErrorExitCode;
        }
    }

    private static int RunCommandLineParser(string[] args)
    {
        return Parser.Default.ParseArguments<CommandLineOptions>(args)
            .WithParsed(opt =>
            {
                ExportPhrasebookToFile(opt.SpreadsheetId, opt.OutputFilePath);
            })
            .WithNotParsed(errors =>
            {
                foreach (var error in errors)
                {
                    Console.WriteLine($"Command line parser error: {error}");
                }
            })
            .MapResult(_ => 0, _ => 1);
    }

    private static void ExportPhrasebookToFile(string spreadsheetId, string outputFilePath)
    {
        var phraseTranslations = LoadPhraseTranslations(spreadsheetId);
        Console.WriteLine($"Successfully loaded phrase book from spreadSheet {spreadsheetId}");
        SavePhraseTranslationsToFile(phraseTranslations, outputFilePath);
        Console.WriteLine($"Successfully saved phrase book to file {outputFilePath}");
    }

    private static IReadOnlyCollection<PhraseTranslation> LoadPhraseTranslations(
        string spreadsheetId)
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
            new PhraseTranslation
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
            .CreateLogger(typeof(Program).Namespace!);

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
        return new SheetsService(initializer);
    }

    private static GoogleCredential LoadCredential()
    {
        var scope = SheetsService.Scope.SpreadsheetsReadonly;
        var credentialFullFilePath = Path.GetFullPath(CredentialFileName, AppContext.BaseDirectory);
        using var stream = new FileStream(credentialFullFilePath, FileMode.Open, FileAccess.Read);
        return GoogleCredential.FromStream(stream).CreateScoped(scope);
    }
}
