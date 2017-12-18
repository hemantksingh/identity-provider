.PHONY: build test run

build:
	cd src/identity-provider && dotnet build

test:
	cd test/identity-provider-test && \
	dotnet test

run:
	cd src/identity-provider && \
	dotnet run