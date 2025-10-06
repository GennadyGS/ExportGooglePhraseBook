param (
    [Parameter(Mandatory=$true)] $spreadSheetId
)

$ErrorActionPreference = "Stop"
. $PSScriptRoot/Common.ps1

Invoke-ExternalCommand dotnet run "--project" $googleDriveFileControlAppPath `
    "--no-build" `
    "--" "-a" Delete "-i" $spreadSheetId
