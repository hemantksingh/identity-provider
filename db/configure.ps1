param(
    [Parameter(Mandatory = $true)][string] $dbServer,
    [Parameter(Mandatory = $true)][string] $dbName,
    [Parameter(Mandatory = $true)][string] $dbUser,
    [Parameter(Mandatory = $true)][string] $dbPassword
)
$ErrorActionPreference = "Stop"

$currentDir = Split-Path $script:MyInvocation.MyCommand.Path
. $currentDir\sqlserver.ps1

function Configure-DbMigrationUser(
    [Parameter(mandatory = $true)][Microsoft.SqlServer.Management.Smo.Database]$database,
    [Parameter(mandatory = $true)][string] $dbUser) {
    Add-UserToDb $database $dbUser
    Add-UserToDbRole $database $dbUser "db_datareader"
    Add-UserToDbRole $database $dbUser "db_datawriter"
    Add-UserToDbRole $database $dbUser "db_ddladmin"
}

function Configure-TestUser(
    [Parameter(mandatory = $true)][Microsoft.SqlServer.Management.Smo.Database]$database,
    [Parameter(mandatory = $true)][string] $dbUser) {
    Add-UserToDb $database $dbUser 

    $roles = @('db_datareader','db_datawriter','db_ddladmin','db_ownner')

    Add-UserToDbRole $database $dbUser "db_datareader"
    Add-UserToDbRole $database $dbUser "db_datawriter"
    Add-UserToDbRole $database $dbUser "db_ddladmin"
    Add-UserToDbRole $database $dbUser "db_owner"
}

[reflection.assembly]::LoadWithPartialName("Microsoft.SqlServer.Smo")
$server = new-object Microsoft.SqlServer.Management.Smo.Server($dbServer)

$userLogin = Create-Login $server $dbUser $dbPassword
Add-LoginToServerRole $server $userLogin.Name "dbcreator"

$db = Create-Db $server $dbName
Configure-TestUser $db $dbUser
