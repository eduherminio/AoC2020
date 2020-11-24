# Install-Module -Name Microsoft.PowerShell.SecretsManagement -RequiredVersion 0.2.0-alpha1 -AllowPrerelease

Import-Module Microsoft.PowerShell.SecretsManagement

if ($args.Count -eq 0) {
	Write-Host "Please provide the day number";
	exit 1;
}

$day = $args[0]

$outputFile = If ($day -ge 10) { "$day.txt" } Else { "0$day.txt" }
$outputPath = "./src/AoC_2020/Inputs/$outputFile"

$url = "https://adventofcode.com/2020/day/$day/input"

$cookie = New-Object System.Net.Cookie
$cookie.Name = "session"
$cookie.Value = (Get-Secret AoCSessionId -AsPlainText)
$cookie.Domain = "adventofcode.com"

$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$session.Cookies.Add($cookie);

Invoke-WebRequest $url -WebSession $session -TimeoutSec 5 -OutFile $outputPath

if (Test-Path $outputPath) {
	Write-Host "Input of Day $day downloaded to $outputPath"
}