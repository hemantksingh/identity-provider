.PHONY: build test run migrate-db

build:
	cd src/identity-provider && dotnet build

test:
	cd test/identity-provider-test && \
	dotnet test

run:
	cd src/identity-provider && \
	dotnet run

migrate-db:
	cd src/identity-provider-sql-migrations && dotnet build
	~/.nuget/packages/fluentmigrator.console/3.0.0/net461/any/Migrate.exe \
	--target="src\identity-provider-sql-migrations\bin\Debug\netcoreapp2.0\identity-provider-sql-migrations.dll" \
	--db=SqlServer \
	-c="Server=localhost;Database=identity;Trusted_Connection=True;"