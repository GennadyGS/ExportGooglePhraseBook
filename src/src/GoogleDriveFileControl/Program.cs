using CommandLine;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using GoogleDriveFileControl.Models;

namespace GoogleDriveFileControl;

internal static class Program
{
    private const int ErrorExitCode = 2;
    private const string SecretFileName = "client_secret.json";
    private static readonly string[] Scopes = [DriveService.Scope.Drive];

    public static async Task<int> Main(string[] args)
    {
        try
        {
            return await RunCommandLineParserAsync(args);
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync(e.ToString());
            return ErrorExitCode;
        }
    }

    private static async Task<int> RunCommandLineParserAsync(string[] args) =>
        (await Parser.Default.ParseArguments<CommandLineOptions>(args)
            .WithParsedAsync(async opt => await RunAsync(opt)))
        .WithNotParsed(errors =>
        {
            foreach (var error in errors)
            {
                Console.WriteLine($"Command line parser error: {error}");
            }
        })
        .MapResult(_ => 0, _ => 1);

    private static async Task RunAsync(CommandLineOptions opt)
    {
        switch (opt.Action)
        {
            case FileAction.Share:
                if (string.IsNullOrEmpty(opt.UserEmail))
                {
                    throw new InvalidOperationException("User email is required for sharing.");
                }

                await ShareFileAsync(opt.FileId, opt.UserEmail);
                break;
            case FileAction.Delete:
                await DeleteFileAsync(opt.FileId);
                break;
            default:
            {
                throw new InvalidOperationException("Invalid action. Use 'share' or 'delete'.");
            }
        }
    }

    private static async Task<Permission> ShareFileAsync(string fileId, string userEmail)
    {
        try
        {
            var service = await AuthenticateUserAsync();
            var result = await ShareFileAsync(service, fileId, userEmail);
            Console.WriteLine(
                $"Successfully shared file (ID: {fileId}) with user {userEmail}. " +
                $"Permission ID: {result.Id}");
            return result;
        }
        catch (Exception e)
        {
            throw new AggregateException(
                $"Error sharing file (ID: {fileId}) with user {userEmail}", e);
        }
    }

    private static async Task DeleteFileAsync(string fileId)
    {
        try
        {
            var service = await AuthenticateUserAsync();
            await DeleteFileAsync(service, fileId);
            Console.WriteLine($"Successfully deleted file (ID: {fileId}).");
        }
        catch (Exception e)
        {
            throw new AggregateException($"Error deleting file (ID: {fileId})", e);
        }
    }

    private static async Task<DriveService> AuthenticateUserAsync()
    {
        var secretFilePath = GetFullFilePath(SecretFileName);
        await using var stream = new FileStream(secretFilePath, FileMode.Open, FileAccess.Read);
        var secretsStream = await GoogleClientSecrets.FromStreamAsync(stream);
        var credential = await GoogleWebAuthorizationBroker
            .AuthorizeAsync(secretsStream.Secrets, Scopes, "user", CancellationToken.None);

        var assemblyFullName = typeof(Program).Assembly.GetName().Name;
        return new DriveService(
            new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = assemblyFullName,
            });
    }

    private static async Task<Permission> ShareFileAsync(
        DriveService service, string fileId, string userEmail)
    {
        var permission = new Permission
        {
            Type = "user",
            Role = "reader",
            EmailAddress = userEmail,
        };

        var request = service.Permissions.Create(permission, fileId);
        request.Fields = "id";
        return await request.ExecuteAsync();
    }

    private static async Task DeleteFileAsync(DriveService service, string fileId)
    {
        try
        {
            var request = service.Files.Delete(fileId);
            await request.ExecuteAsync();
        }
        catch (Exception e)
        {
            throw new AggregateException($"Error deleting file (ID: {fileId})", e);
        }
    }

    private static string GetFullFilePath(string filePath)
    {
        var assemblyPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
        return Path.Combine(assemblyPath ?? string.Empty, filePath);
    }
}
