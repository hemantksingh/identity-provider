param(
    [Parameter(Mandatory = $true)][string] $dbServer,
    [Parameter(Mandatory = $true)][string] $dbName,
    [Parameter(Mandatory = $true)][string] $dbUser,
    [Parameter(Mandatory = $true)][string] $dbPassword
)
$ErrorActionPreference = "Stop"

$currentDir = Split-Path $script:MyInvocation.MyCommand.Path
. $currentDir\sqlserver.ps1

# Db migration user does not necessarily need a 'db_owner' role
# It is safe to add this role assigned for now, coz the user gets 
# cleaned up post migration. This also ensures a single user configuration
function Configure-User(
    [Parameter(mandatory = $true)][Microsoft.SqlServer.Management.Smo.Database]$database,
    [Parameter(mandatory = $true)][string] $dbUser) {
    
    Add-UserToDb $database $dbUser
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
Configure-User $db $dbUser
