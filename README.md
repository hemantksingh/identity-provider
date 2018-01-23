# IDP using IdentityServer 4

## Requirements

* dotnet core `choco install dotnetcore-sdk -v`
* GNU Make `choco install make -v`

## Build

`make build`

## Run

```sh
make run
curl http://localhost:49842/.well-known/openid-configuration
```

## Test

`make test`

## Migrate Database

`make migrate-db`