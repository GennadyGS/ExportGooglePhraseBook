# Export Google Phrasebook

## Repository layout

- [ExportGooglePhraseBook.slnx](ExportGooglePhraseBook.slnx): solution entry point.
- [src](src): .NET libraries, command-line applications, and browser extension.
- [scripts](scripts): export, spreadsheet deletion, and custom URL protocol scripts.
- [eng/analyzers](eng/analyzers): shared analyzer rules and MSBuild targets.
- [Directory.Build.props](Directory.Build.props) and [Directory.Packages.props](Directory.Packages.props): repository-wide build settings and centrally managed package versions.

Install .NET SDK 10.0.404 or a newer .NET 10 SDK. [global.json](global.json) keeps builds on .NET 10 even when .NET 11 is installed. Build from the repository root:

```powershell
dotnet build .\ExportGooglePhraseBook.slnx --configuration Release
```

The browser extension output is generated under `src\ExportGooglePhraseBookExtension\bin\Release\<target-framework>\browserextension`. Build output, local configuration, and credentials are ignored by Git.

### Browser extension troubleshooting

WebAssembly Hot Reload is disabled in the [extension project](src/ExportGooglePhraseBookExtension/ExportGooglePhraseBookExtension.csproj). Its Manifest V3 background service worker has no DOM, and the Blazor browser-extension DOM proxy incorrectly handles the Hot Reload module's browser-refresh script lookup, logging `Unexpected selector script[src*='aspnetcore-browser-refresh']`.

Clean and rebuild the extension once to remove stale Hot Reload artifacts:

```powershell
dotnet clean .\src\ExportGooglePhraseBookExtension\ExportGooglePhraseBookExtension.csproj --configuration Debug
dotnet build .\src\ExportGooglePhraseBookExtension\ExportGooglePhraseBookExtension.csproj --configuration Debug
```

Reload the unpacked extension from the matching `browserextension` output directory in your browser's extension management page and clear the previously recorded errors. Debug builds remain available, but applying code changes requires rebuilding and reloading the extension.

## Configuration

1. Generate `credential.json` (service account key) and put it in [src/ExportGooglePhraseBookFromSpreadSheet](src/ExportGooglePhraseBookFromSpreadSheet):
   - Go to <https://console.cloud.google.com/iam-admin/serviceaccounts>, select the project.
   - Pick the service account (or create one) -> Actions (⋮) -> Manage keys -> Add key -> Create new key -> JSON.
   - Download the file, rename it to `credential.json`, and place it in the folder above.
2. Generate `client_secret.json` (OAuth client) and put it in [src/GoogleDriveFileControl](src/GoogleDriveFileControl):
   - Go to <https://console.cloud.google.com/apis/credentials>, select the project.
   - Ensure the Google Drive API is enabled (APIs & Services -> Library -> Google Drive API -> Enable).
   - Configure the OAuth consent screen if not already done (add your account as a test user if in "Testing" mode).
   - Create Credentials -> OAuth client ID -> Application type: Desktop app -> Create.
   - Download the JSON, rename it to `client_secret.json`, and place it in the folder above.

Both files contain secrets and must never be committed to source control (already covered by `.gitignore`).

## Running scripts

Export a spreadsheet from the repository root:

```powershell
.\scripts\ExportGooglePhrasebook.ps1 -spreadSheetId '<spreadsheet-id>' -outputFilePath '.\Output\GooglePhrasebook.json'
```

To register the custom URL protocol, run:

```powershell
.\scripts\CustomUrlProtocol\RegisterCustomUrlProtocol.ps1
```

After updating from the old layout, move existing local credentials from `src\src\<project>` to `src\<project>` and rerun protocol registration to replace the stored script path. The URL handler still writes to the repository-root `Output` directory. Consumers must update project references, analyzer imports, and script invocations to the new paths.
