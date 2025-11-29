using CommandLine;
using ExportGooglePhraseBookFromSpreadSheet.Configuration;
using ExportGooglePhraseBookFromSpreadSheet.Services;

namespace ExportGooglePhraseBookFromSpreadSheet;

public static class Program
{
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
                PhrasebookExporter.Export(opt.SpreadsheetId, opt.OutputFilePath);
                Console.WriteLine($"Successfully loaded phrase book from spreadSheet {opt.SpreadsheetId}");
                Console.WriteLine($"Successfully saved phrase book to file {opt.OutputFilePath}");
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
}
