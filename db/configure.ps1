param(
    [Parameter(Mandatory=$true)][string] $dbServer,
    [Parameter(Mandatory=$true)][string] $dbName,
    [Parameter(Mandatory=$true)][string] $dbUser,
    [Parameter(Mandatory=$true)][string] $dbPassword
)

$currentDir = Split-Path $script:MyInvocation.MyCommand.Path
. $currentDir\sqlserver.ps1

[reflection.assembly]::LoadWithPartialName("Microsoft.SqlServer.Smo")
$server = new-object Microsoft.SqlServer.Management.Smo.Server($dbServer)

$db = Get-Db $server $dbName

Write-Host $db