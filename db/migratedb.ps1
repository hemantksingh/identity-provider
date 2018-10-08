param (
    $migratorexe = "\.nuget\packages\FluentMigrator.Console.3.0.0\net461\any\Migrate.exe",
    [Parameter(mandatory=$true)]
    [string] $target,
    [Parameter(mandatory=$true)]
    [string] $connectionString,
	[int] $timeout=0
)

[string[]]$arguments = @(
    "--target=$target",
    "--db=SqlServer",
    "--c=$connectionString",
	"--timeout=$timeout"
)

& $migratorexe -ArgumentList $arguments