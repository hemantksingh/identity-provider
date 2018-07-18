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

$userLogin = CreateLogin $server $dbUser $dbPassword

AddLoginToServerRole $server $userLogin.Name "dbcreator"

$db = Create-Db $server $dbName

AddUserToDb $db $dbUser
AddUserToDbRole $db $dbUser "db_datareader"
AddUserToDbRole $db $dbUser "db_datawriter"
AddUserToDbRole $db $dbUser "db_ddladmin"