.PHONY: build test run database

build:
	cd src/identity-provider && dotnet build

test:
	cd test/identity-provider-test && \
	dotnet test

run:
	cd src/identity-provider && \
	dotnet run

DBSERVER ?= localhost
DBNAME ?= identity
CONNECTION := "Server=$(DBSERVER);Database=$(DBNAME);"

ifdef USERID
	CONNECTION_STRING = $(CONNECTION)"User ID=$(USERID);Password=Password12!"
else
	CONNECTION_STRING = $(CONNECTION)"Trusted_Connection=True;"
endif

database:
	cd src/identity-provider-sql-migrations && dotnet build
	~/.nuget/packages/fluentmigrator.console/3.0.0/net461/any/Migrate.exe \
	--target="src\identity-provider-sql-migrations\bin\Debug\netcoreapp2.0\identity-provider-sql-migrations.dll" \
	--db=SqlServer \
	-c=$(CONNECTION_STRING)