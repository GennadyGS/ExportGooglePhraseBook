param (
    $name = "exportGooglePhrasebook",
    $description = "Export Google Phrasebook",
    $scriptPath = "$PSScriptRoot/../ExportGooglePhrasebookCustomUrlHandler.ps1"
)

$url = "pwsh.exe -WindowStyle Minimized `"$scriptPath`" `"%1`""
$classesPath = "HKCU:\SOFTWARE\Classes"
New-Item -Path "$classesPath" -Name "$name" -Force

New-Item -Path "$classesPath\$name" -Name "shell" -Force
Set-ItemProperty -Path "$classesPath\$name" -Name "(Default)" -Value "$description"
New-ItemProperty -Path "$classesPath\$name" -Name "URL Protocol" -Value "" -PropertyType String -Force

New-Item -Path "$classesPath\$name\shell" -Name "open" -Force
New-Item -Path "$classesPath\$name\shell\open" -Name "command" -Force
Set-ItemProperty -Path "$classesPath\$name\shell\open\command" -Name "(Default)" -Value "$url"
