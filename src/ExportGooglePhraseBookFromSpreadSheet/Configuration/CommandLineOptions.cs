using CommandLine;

namespace ExportGooglePhraseBookFromSpreadSheet.Configuration;

internal sealed class CommandLineOptions
{
    [Option('i', "spreadsheetId", HelpText = "Input Spreadsheet ID.", Required = true)]
    public string SpreadsheetId { get; init; } = string.Empty;

    [Option('o', "output", HelpText = "Output file path.", Required = true)]
    public string OutputFilePath { get; init; } = string.Empty;
}
