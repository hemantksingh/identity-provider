# IDP using IdentityServer 4

[Identity Server](https://github.com/IdentityServer/IdentityServer4) enables federated security providing identity as a service for single sign on and access control. This implementation provides integration of the IDP with the following clients:

* Hybrid flow for a WebApp
* Implicit flow for SPA
* Client credentials flow for server to server

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

`make database`

## Appveyor
![Build Status](https://ci.appveyor.com/api/projects/status/github/hemantksingh/identity-provider?branch=master&svg=true)
