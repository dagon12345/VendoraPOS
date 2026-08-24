# Architecture

## 1. Why this layering (Clean Architecture + DDD + Repository)

You already know this from your local POS work — this is the same shape, formalized:

```
Vendora.Domain          <- no project references. Pure C#: entities, invariants, repository interfaces.
     ^
Vendora.Application     <- references Domain. Use-case orchestration (services), DTOs.
     ^
Vendora.Infrastructure  <- references Application + Domain. EF Core, DbContext, repository implementations.
     ^
Vendora.Api             <- references Application + Infrastructure. Controllers, DI wiring, HTTP concerns.
```

The dependency rule: **arrows point inward only**. Domain knows nothing about EF Core or ASP.NET Core —
it could be reused with a different database or even a desktop UI. This is what makes it "clean": business
rules (Domain) don't rot when you change frameworks.

Repository pattern lives at the Domain/Infrastructure boundary:
- `Vendora.Domain/Products/IProductRepository.cs` — the contract, defined where the business logic needs it.
- `Vendora.Infrastructure/Repositories/ProductRepository.cs` — the EF Core implementation.

This is the same pattern you've used before; the only change from a typical n-tier setup is that the
*interface* lives in Domain, not Infrastructure — Infrastructure depends on Domain, never the reverse.

## 2. The vertical slice we built: `Product`

Use this as the template for every future entity (Category, Customer, Sale, Subscription...):

1. **Domain** (`Vendora.Domain/Products/Product.cs`) — the entity, with a private constructor and a
   `Create()` factory method that enforces invariants (no negative price, required SKU, etc.). Behavior
   like `AdjustStock()` lives on the entity itself, not in a service — this is the DDD idea of a "rich
   domain model" instead of an anemic one with public setters everywhere.
2. **Domain** (`IProductRepository.cs`) — what persistence operations the business needs, expressed in
   business language (`GetBySkuAsync`), not database language.
3. **Application** (`ProductService.cs`) — orchestrates one use case per method: load from the repository,
   call domain methods, save, map to a DTO. This is the layer your controllers talk to; it has zero
   knowledge of HTTP or SQL.
4. **Infrastructure** (`Persistence/VendoraDbContext.cs`, `Configurations/ProductConfiguration.cs`,
   `Repositories/ProductRepository.cs`) — EF Core specifics: table mapping, indexes, the actual queries.
5. **Api** (`Controllers/ProductsController.cs`) — thin. Each action calls one `IProductService` method
   and translates the result to an HTTP status code. No business logic here.

To add a new entity later, copy this file list and rename. It's mechanical once you've done it once.

## 3. Angular, explained for a C# / DI-minded developer

Angular's concepts map fairly directly onto things you already know:

| Angular concept | .NET equivalent / analogy |
|---|---|
| `@Injectable({ providedIn: 'root' })` class | A service registered as a singleton in DI (`services.AddSingleton<T>()`), except Angular's root injector does this per-app instead of per-request |
| Constructor injection (`constructor(private http: HttpClient)`) | Same as ASP.NET Core constructor injection — Angular has its own DI container |
| `HttpClient` | `HttpClient` in .NET — a typed wrapper for HTTP calls, returns `Observable<T>` instead of `Task<T>` |
| `Observable` (RxJS) | Closest analogy: `IAsyncEnumerable<T>` combined with events — a stream you `.subscribe()` to instead of `await`. For a single HTTP response, think of it as "a `Task<T>` that you must explicitly subscribe to, or nothing happens" |
| Component (`@Component`) | Closest to a Razor Page/View + code-behind combined: `.ts` (logic) + `.html` (template) + `.scss` (styles), scoped together |
| `signal()` | A mutable, observable value cell — think `INotifyPropertyChanged` on a single field, built into the framework. Reading it in a template auto-subscribes the UI to changes. |
| Routes (`app.routes.ts`) | Like ASP.NET Core endpoint routing, but client-side: maps a URL path to a component instead of a controller action |
| `standalone` component | A component that declares its own dependencies (`imports: [...]`) instead of belonging to an `NgModule` — this is the modern default; you'll rarely write `NgModule`s in new Angular code |

### The request flow you just saw working

1. Browser loads `http://localhost:4200` → Angular's router matches `/products` → renders `ProductList`.
2. `ProductList.ngOnInit()` calls `ProductService.getAll()`, which calls `HttpClient.get('/api/products')`.
3. Angular's dev server (`proxy.conf.json`) sees the `/api` prefix and forwards the request to
   `https://localhost:7196/api/products` — this is why the browser code never hardcodes a port.
4. ASP.NET Core routes it to `ProductsController.GetAll()` → `IProductService.GetAllAsync()` →
   `IProductRepository.GetAllAsync()` → EF Core → SQL Server (Docker) → data flows back up.

Everything downstream of step 4 is exactly the layered architecture from section 1.

## 4. General-purpose data model (pharmacy + retail + coffee shop)

`Product` was deliberately kept generic — SKU, name, price, quantity, description — so it works for a can
of soda, a bag of coffee beans, or a box of paracetamol without modification. Vertical-specific behavior
(drug expiry/batch tracking for pharmacy, recipe/modifiers for a cafe) will be added later as **separate,
optional bounded contexts** that reference `Product` by ID rather than fields bolted onto it — this keeps
the core sellable-item model clean and keeps pharmacy-only complexity out of a coffee shop's schema.

## 5. Subscription / multi-tenant plan (not yet built)

For "one codebase, many customer businesses," the two realistic models are:

- **Shared database, `TenantId` column** — every table gets a `TenantId`, every query filters by it
  (EF Core global query filters make this nearly invisible in code). Cheapest to run, easiest to manage
  migrations for. Recommended starting point for a subscription SaaS at your current scale.
- **Database-per-tenant** — stronger isolation, more operational overhead (migrations must run N times).
  Usually only justified by specific compliance requirements or very large tenants.

Given your monsterasp hosting and wanting to onboard many small stores cheaply, shared-DB with `TenantId`
is the pragmatic choice — we'll add it once auth/identity is in place, since the tenant is normally
resolved from the logged-in user's claims.

## 6. What's next (suggested order)

1. Authentication (ASP.NET Core Identity or JWT) + `TenantId` on the entity base class.
2. A second vertical slice (e.g. `Category` or `Sale`) done end-to-end by *you*, using this doc as the
   checklist, with me reviewing rather than writing it — the fastest way to actually learn the Angular half.
3. Wire the Angular client's `environment.production.ts` to point at your monsterasp-hosted API, and get
   a deploy pipeline (GitHub Actions) publishing both API and Angular build to the host.
