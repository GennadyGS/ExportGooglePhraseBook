using CommandLine;

namespace GoogleDriveFileControl.Models;

internal sealed record CommandLineOptions
{
    [Option('a', "action", HelpText = "Action to perform: share or delete.", Required = true)]
    public required FileAction Action { get; init; }

    [Option('i', "fileId", HelpText = "Input File ID.", Required = true)]
    public required string FileId { get; init; }

    [Option('u', "userEmail", HelpText = "User Email.")]
    public string? UserEmail { get; init; }
}
