.PHONY: build test package run database

APPLICATION=identity-provider
BUILD_NUMBER?=0
APP_VERSION=1.0.0.$(BUILD_NUMBER)
CONFIGURATION?=Debug
PUBLISH_DIR=${CURDIR}/out

build:
	cd src/identity-provider && dotnet build

test:
	cd test/identity-provider-test && \
	dotnet test

package:
	dotnet publish -o $(PUBLISH_DIR) -c $(CONFIGURATION) src/$(APPLICATION)
	powershell "'$(APP_VERSION)' | out-file '$(PUBLISH_DIR)\version.txt'"
	powershell ./choco/chocopack.ps1 -application $(APPLICATION) -version $(APP_VERSION) -publishDir $(PUBLISH_DIR)

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