using Microsoft.EntityFrameworkCore;
using Vendora.Domain.Products;
using Vendora.Infrastructure.Persistence;

namespace Vendora.Infrastructure.Repositories;

public class ProductRepository(VendoraDbContext context) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Products.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default) =>
        await context.Products.AsNoTracking().ToListAsync(ct);

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default) =>
        await context.Products.FirstOrDefaultAsync(p => p.Sku == sku, ct);

    public async Task AddAsync(Product entity, CancellationToken ct = default) =>
        await context.Products.AddAsync(entity, ct);

    public void Update(Product entity) => context.Products.Update(entity);

    public void Remove(Product entity) => context.Products.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
}
