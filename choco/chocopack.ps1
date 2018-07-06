param(
    [Parameter(Mandatory=$true)][string] $application,
    [Parameter(Mandatory=$true)][string] $version,
    [Parameter(Mandatory=$true)][string] $publishDir
)

$currentDir = $PSScriptRoot
$nuspec = "$currentDir\$application.nuspec"
$artifact = "$currentDir\tools\$application.zip"

$ErrorActionPreference = "Stop"
if (Test-Path $artifact) {Remove-item  $artifact -ErrorAction SilentlyContinue}
(Get-Content $nuspec) -replace "<version>.*</version>", "<version>$($version)</version>" | Set-Content $nuspec

Write-Host "Archiving contents of '$publishDir' to '$artifact'"
7z a -tzip -xr@"$currentDir\excludelist.txt" $artifact $publishDir/*

choco pack $nuspec