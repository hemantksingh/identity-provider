param(
    [Parameter(Mandatory=$true)][string] $dbServer,
    [Parameter(Mandatory=$true)][string] $dbName,
    [Parameter(Mandatory=$true)][string] $dbUser,
    [Parameter(Mandatory=$true)][string] $dbPassword
)
$ErrorActionPreference = "Stop"

$currentDir = Split-Path $script:MyInvocation.MyCommand.Path
. $currentDir\sqlserver.ps1

[reflection.assembly]::LoadWithPartialName("Microsoft.SqlServer.Smo")
$server = new-object Microsoft.SqlServer.Management.Smo.Server($dbServer)

$userLogin = Create-Login $server $dbUser $dbPassword

Add-LoginToServerRole $server $userLogin.Name "dbcreator"

$db = Create-Db $server $dbName

Add-UserToDb $db $dbUser
Add-UserToDbRole $db $dbUser "db_datareader"
Add-UserToDbRole $db $dbUser "db_datawriter"
Add-UserToDbRole $db $dbUser "db_ddladmin"