# VendoraPOS

A cloud-based, subscription POS platform for pharmacies, small retail stores, and coffee shops.
Backend: .NET (Clean Architecture + DDD + Repository pattern). Frontend: Angular.

See [docs/architecture.md](docs/architecture.md) for the full design rationale and folder-by-folder explanation,
[docs/user-guide.md](docs/user-guide.md) for plain-language instructions on using each feature, or
[CONTRIBUTING.md](CONTRIBUTING.md) if you're setting up locally or opening a pull request.

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

**API**:
```
./scripts/run-api.sh          # dotnet run — single start
./scripts/run-api.sh watch    # dotnet watch run — auto-rebuild/restart on file changes
```
Listens on `https://localhost:7196`. This frees ports 7196/5136 first, so it's safe to re-run even if a
previous instance (e.g. one started in the background by a tool) is still holding the port — `Ctrl+C` only
kills a foreground process in your own terminal, so a detached instance needs this instead.
The local DB connection string lives in `appsettings.json` for development (real production values go in
a gitignored `appsettings.Production.json` instead).

If running manually instead of via the script (from `src/server/Vendora.Api`), always pass the `https`
launch profile — otherwise the API only binds to `http://localhost:5136`, and the Angular proxy (which
targets `https://localhost:7196`) can't reach it:
```
dotnet run --launch-profile https
```

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
- [x] `Product` CRUD (create/edit/activate-deactivate) working end-to-end (Domain → Application → Infrastructure → API → Angular)
- [x] `StockMovement` ledger — Restock/Adjustment/Waste/InitialStock, with a "Reverse" correction flow
- [x] `ProductAuditLog` — records every product edit (field-level diff) and activate/deactivate
- [x] Responsive UI (mobile/tablet), confirmation modals for edit/deactivate, in-app Help page + exportable user guide
- [ ] Authentication / multi-tenant subscription model
- [ ] Category / other entities beyond Product — see [docs/modules/category-module.md](docs/modules/category-module.md) for a ready-to-hand-off brief
- [ ] Sales / checkout module (will need a "Sale" stock-movement reason once built)
- [ ] Deploy to hosting (monsterasp) + GitHub CI

Note: there is deliberately no "delete a product" feature — every product picks up stock history almost
immediately, making hard deletion impractical. **Deactivate** is the supported way to retire a product.
