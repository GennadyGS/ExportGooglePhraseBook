param (
    [Parameter(Mandatory=$true)] $url
)

Remove-Item Env:PLATFORM -ErrorAction SilentlyContinue

Add-Type -AssemblyName System.Web

$queryString = [System.Web.HttpUtility]::ParseQueryString($url.Split('?')[1])
$spreadSheetId = $queryString["spreadSheetId"]
& $PSScriptRoot/ExportGooglePhrasebook.ps1 `
    -spreadSheetId $spreadSheetId `
    -outputFilePath "$PSScriptRoot/Output/GooglePhrasebook.json"
