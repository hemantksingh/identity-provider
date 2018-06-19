# Multitenant Idendity Provider

Identity Provider is a multi-tenant MVC web app for login, logout and consent based on [Identity Server 4](https://github.com/IdentityServer/IdentityServer4). Identity Server is an open id connect framework that extends ASP.NET Core authentication system to enable federated security, providing identity as a service for token based authentication, single sign on and access control.  This implementation provides integration of the IDP with the following clients:

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

## Identity database

Refernce [Sample data schema](https://github.com/aspnet/Docs/blob/master/aspnetcore/security/authentication/identity/sample/src/ASPNETCore-IdentityDemoComplete/IdentityDemo/Data/Migrations/00000000000000_CreateIdentitySchema.cs)

`make database`

## Appveyor

![Build Status](https://ci.appveyor.com/api/projects/status/github/hemantksingh/identity-provider?branch=master&svg=true)