# Module brief: Category

**Status:** not started — this is a handoff brief, not a plan someone has already begun.
**Read [CONTRIBUTING.md](../../CONTRIBUTING.md) first** — this module must follow those conventions exactly.

## Why this module

`Product` is the only entity that exists so far. Categories let a store group products (e.g. "Beverages",
"Pain Relief", "Snacks") for browsing/reporting later. This is explicitly the "second vertical slice" the
architecture doc calls for — the goal is to prove the `Product` pattern generalizes to a new entity,
end-to-end, not to build anything Category-specific or clever.

**Do not start Sales/checkout, Auth, or anything else — this module is Category only.**

## Scope

In scope:
- `Category` CRUD (create, list, edit, delete) — Domain → Application → Infrastructure → Api → Angular.
- Optionally assigning a `Product` to a `Category` (nullable `CategoryId` on `Product`).
- Filtering the product list by category in the UI (nice-to-have, not required for "done").

Out of scope (do not build these now):
- Subcategories / nested categories.
- Anything related to Sales, Auth, or multi-tenancy.
- Deleting a category that has products assigned to it should be handled explicitly (see below) — don't
  guess at a general "safe delete" framework beyond what's described here.

## Backend — mirror the `Product` slice exactly

Look at these existing files as the literal template — same folder shape, same style:
- `src/server/Vendora.Domain/Products/Product.cs`, `IProductRepository.cs`
- `src/server/Vendora.Application/Products/ProductDto.cs`, `IProductService.cs`, `ProductService.cs`
- `src/server/Vendora.Infrastructure/Persistence/Configurations/ProductConfiguration.cs`, `Repositories/ProductRepository.cs`
- `src/server/Vendora.Api/Controllers/ProductsController.cs`

### `Category` entity (`Vendora.Domain/Categories/Category.cs`)
- Fields: `Name` (string, required, unique), `Description` (string?, optional).
- Private constructor + static `Create(name, description)` factory validating name is non-empty.
- `UpdateDetails(name, description)` method — same pattern as `Product.UpdateDetails`.
- No delete-blocking logic in the entity itself — that's an Application-layer concern (see below).

### `IProductRepository`-equivalent
- `ICategoryRepository : IRepository<Category>` (the generic base already gives you Get/Add/Update/Remove/SaveChanges).

### Application layer
- `CategoryDto(Guid Id, string Name, string? Description, int ProductCount)` — include a product count so the UI can warn before deleting a category that's in use. (`ProductCount` computed in the service, not stored on the entity.)
- `CreateCategoryRequest(string Name, string? Description)`, `UpdateCategoryRequest(string Name, string? Description)`.
- `ICategoryService` / `CategoryService` with `GetAllAsync`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`.
- **Delete behavior — decide and implement one of these, don't leave it ambiguous:**
  - Recommended: if `ProductCount > 0`, refuse deletion in the service (return a result the controller turns into `409 Conflict` with a clear message — same pattern as how the old delete-with-history conflict used to work for Products, see git history of `ProductsController.cs` if useful as a reference for the shape, even though that code path no longer exists).
  - This means `CategoryService.DeleteAsync` needs to check product count via `IProductRepository` (inject it alongside `ICategoryRepository`), not just `ICategoryRepository` alone.

### `Product` gets an optional category
- Add nullable `CategoryId` (Guid?) to `Product` — a new method `Product.AssignCategory(Guid? categoryId)` (don't put this in the constructor/`Create`, it's a separate concern from creation).
- Update `ProductDto` to include `CategoryId` and (optionally) `CategoryName` for display.
- Update `ProductConfiguration` with the FK: `HasOne<Category>().WithMany().HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.SetNull)` — so deleting a category (once allowed, i.e. `ProductCount == 0`) never orphans a reference; and a product simply reverts to "no category" if this ever needs to change later.

### Infrastructure
- `CategoryConfiguration`, `CategoryRepository` — mirror `ProductConfiguration`/`ProductRepository`.
- Register everything in `DependencyInjection.cs`.
- **New EF Core migration** (from `src/server/Vendora.Api`):
  ```
  dotnet ef migrations add AddCategory --project ../Vendora.Infrastructure --startup-project . --output-dir Persistence/Migrations
  dotnet ef database update --project ../Vendora.Infrastructure --startup-project .
  ```

### Api
- `CategoriesController` — `GET /api/categories`, `GET /api/categories/{id}`, `POST`, `PUT`, `DELETE` — mirror `ProductsController`'s style (thin actions, `NotFound()`/`Ok()`/`CreatedAtAction`).
- `DELETE` returns `409 Conflict` with a message when the category still has products (per the Application-layer decision above).
- Extend `UpdateProductRequest`/the product edit endpoint to accept an optional `categoryId` so a product can be (re)assigned a category.

## Frontend — mirror the `Product` feature folder exactly

Look at `src/client/src/app/features/inventory/product-list/` and `product-form/` as the template.

- `core/models/category.model.ts`, `core/services/category.ts` (mirror `product.model.ts`/`product.ts`).
- `features/categories/category-list/` — table of categories (use the `.responsive-table` class + `data-label` attrs, same as `product-list.html`), with product count shown per row, and Edit/Delete buttons. Delete should surface the 409 message the same way `product-list.ts` currently surfaces its `actionError` banner (reuse that pattern — dismissible banner over the still-visible table, not a page-replacing error).
- `features/categories/category-form/` — create/edit form, mirror `product-form.ts`'s structure (`inject()` for `FormBuilder`, edit-mode load-and-patch, use the existing `ConfirmDialog` component for edit confirmation the same way `product-form` does now).
- Add a Category `<select>` (optional, "No category" as the default option) to `product-form.html`, wired to the new `categoryId` field.
- Add routes: `/categories`, `/categories/new`, `/categories/:id/edit`.
- Add a "Categories" link to the header nav in `app.html`, next to "Products".
- (Nice-to-have, not required for done) a category filter dropdown above the product table in `product-list.html`.

## Docs

Per `CONTRIBUTING.md`'s "3 places in sync" rule, add a "Categories" section to:
1. `docs/user-guide.md`
2. `src/client/src/app/features/help/help.html`
3. `docs/user-guide.html` (regenerate the PDF/Word exports in `docs/exports/` afterward — see the LibreOffice commands documented in `CONTRIBUTING.md`/the memory note referenced there, or ask if you don't have LibreOffice installed).

## Definition of done

- `dotnet build` and `ng build` both pass cleanly.
- Full CRUD works clicked-through in the browser: create a category, assign it to a product via the product edit form, see the category name reflected somewhere in the product list or product detail, edit a category, and confirm deleting a category **with** products assigned is blocked with a clear message while deleting an **empty** category succeeds.
- Mobile layout checked at <640px (category table collapses to cards, forms usable).
- User guide updated in all 3 places.
- Opened as a PR against `master` — do not push directly, branch protection will block it anyway.
