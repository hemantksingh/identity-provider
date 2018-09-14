param(
    [Parameter(Mandatory=$true)][string] $dbServer,
    [Parameter(Mandatory=$true)][string] $dbName,
    [Parameter(Mandatory=$true)][string] $dbUser
)
$ErrorActionPreference = "Stop"

$currentDir = Split-Path $script:MyInvocation.MyCommand.Path
. $currentDir\sqlserver.ps1

[reflection.assembly]::LoadWithPartialName("Microsoft.SqlServer.Smo")
$server = new-object Microsoft.SqlServer.Management.Smo.Server($dbServer)

Remove-LoginFromServer $server $dbUser

$db = Get-Db $server $dbName

Remove-UserFromDb $db $dbUser