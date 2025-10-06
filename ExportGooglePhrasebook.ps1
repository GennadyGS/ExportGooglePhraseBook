param (
    [Parameter(Mandatory=$true)] $spreadSheetId,
    [Parameter(Mandatory=$true)] $outputFilePath
)

$ErrorActionPreference = "Stop"
. $PSScriptRoot/Common.ps1

$exportGooglePhraseBookFromSpreadSheetAppPath =
    "$appPathRoot/ExportGooglePhraseBookFromSpreadSheet"

$credentialFilePath = "$exportGooglePhraseBookFromSpreadSheetAppPath/credential.json"
$credential = Get-Content $credentialFilePath -Raw | ConvertFrom-Json

$outputPath = Split-Path $outputFilePath -Parent
if (!(Test-Path $outputPath -PathType Container)) {
  New-Item $outputPath -ItemType Directory | Out-Null
}

Invoke-ExternalCommand dotnet run "--project" $googleDriveFileControlAppPath `
    "--" "-a" Share "-i" $spreadSheetId "-u" $credential.client_email

Invoke-ExternalCommand dotnet run "--project" $exportGooglePhraseBookFromSpreadSheetAppPath `
    "--" "-i" $spreadSheetId "-o" $outputFilePath
