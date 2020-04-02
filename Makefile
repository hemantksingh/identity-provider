.PHONY: build test package run database

APPLICATION=identity-provider
BUILD_NUMBER?=0
APP_VERSION=1.0.$(BUILD_NUMBER)
CONFIGURATION?=Debug
PUBLISH_DIR=${CURDIR}/out

DBSERVER ?= localhost
DBNAME ?= identity
CONNECTION := "Server=$(DBSERVER);Database=$(DBNAME);"

ifdef DBUSER
	CONNECTION_STRING = $(CONNECTION)"User ID=$(DBUSER);Password=$(DBPASSWORD)"
else
	CONNECTION_STRING = $(CONNECTION)"Trusted_Connection=True;"
endif

build:
	cd src/identity-provider && dotnet build

test:
	cd test/tests-identity-provider && \
	dotnet test

package:
	dotnet publish -o $(PUBLISH_DIR) -c $(CONFIGURATION) src/$(APPLICATION)
	powershell "'$(APP_VERSION)' | out-file '$(PUBLISH_DIR)\version.txt'"
	powershell ./choco/chocopack.ps1 -application $(APPLICATION) -version $(APP_VERSION) -publishDir $(PUBLISH_DIR)

run:
	cd src/identity-provider && \
	dotnet run

SQLSERVER_PACKAGE=shellpower.sqlserver
SQLSERVER_PACKAGE_VERSION=1.0.9
PACAKGE=$(SQLSERVER_PACKAGE).$(SQLSERVER_PACKAGE_VERSION)
configure-db:
	nuget install $(SQLSERVER_PACKAGE) -version $(SQLSERVER_PACKAGE_VERSION) -outputdirectory $(PUBLISH_DIR)
ifdef DBUSER
	powershell ". $(PUBLISH_DIR)\$(PACAKGE)/bin/sqlserver.ps1 -dbServer \"$(DBSERVER)\" -dbName $(DBNAME); Add-DbUser -name \"$(DBUSER)\" -password \"$(DBPASSWORD)\" -serverRoles @(\"dbcreator\", \"sysadmin\")"
else
	@echo "Using trusted connection"
endif

database: configure-db
	cd src/identity-provider-sql-migrations && dotnet build
	~/.nuget/packages/fluentmigrator.console/3.0.0/net461/any/Migrate.exe \
	--target="src\identity-provider-sql-migrations\bin\Debug\netcoreapp2.0\identity-provider-sql-migrations.dll" \
	--db=SqlServer \
	-c=$(CONNECTION_STRING)

cleanup-db:
	powershell ". $(PUBLISH_DIR)\$(PACAKGE)/bin/sqlserver.ps1 -dbServer \"$(DBSERVER)\" -dbName $(DBNAME); Remove-DbUser \"$(DBUSER)\""