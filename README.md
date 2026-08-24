# VendoraPOS

A cloud-based, subscription POS platform for pharmacies, small retail stores, and coffee shops.
Backend: .NET (Clean Architecture + DDD + Repository pattern). Frontend: Angular.

See [docs/architecture.md](docs/architecture.md) for the full design rationale and folder-by-folder explanation.

## Repository layout

```
VendoraPOS/
  Vendora.slnx                  .NET solution file
  src/
    server/
      Vendora.Domain/           Entities, value objects, repository interfaces. No dependencies.
      Vendora.Application/      Use-case services, DTOs. Depends on Domain only.
      Vendora.Infrastructure/   EF Core, DbContext, repository implementations. Depends on Application + Domain.
      Vendora.Api/              ASP.NET Core Web API (controllers). Depends on Application + Infrastructure.
    client/                     Angular app (feature-folder structure)
  tests/
    Vendora.Domain.Tests/
    Vendora.Application.Tests/
  docs/
    architecture.md
```

## Prerequisites (already installed on this machine)

- .NET 10 SDK
- Node.js 24 (via nvm) + Angular CLI
- Docker, running a local SQL Server container (`eca-mssql`, port 1433)
- `dotnet-ef` global tool

## Running locally

**API** (from `src/server/Vendora.Api`):
```
dotnet run --launch-profile https
```
Listens on `https://localhost:7196`. The local DB connection string lives in `dotnet user-secrets`
(never committed) — see `dotnet user-secrets list` in that project folder.

**Client** (from `src/client`):
```
npm start
```
Serves on `http://localhost:4200` with `proxy.conf.json` forwarding `/api/*` to the API,
so the browser never needs CORS or to know the API's port.

## Database migrations

From `src/server/Vendora.Api`:
```
dotnet ef migrations add <Name> --project ../Vendora.Infrastructure --startup-project . --output-dir Persistence/Migrations
dotnet ef database update --project ../Vendora.Infrastructure --startup-project .
```

## Status

- [x] Solution scaffolded (Clean Architecture, 4 layers)
- [x] Angular workspace scaffolded (feature-folder layout)
- [x] First vertical slice working end-to-end: `Product` CRUD (Domain → Application → Infrastructure → API → Angular)
- [ ] Authentication / multi-tenant subscription model
- [ ] Sales / checkout module
- [ ] Deploy to hosting (monsterasp) + GitHub CI
