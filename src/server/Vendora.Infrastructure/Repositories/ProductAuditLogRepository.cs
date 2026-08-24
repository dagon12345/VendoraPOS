using Microsoft.EntityFrameworkCore;
using Vendora.Domain.Products;
using Vendora.Infrastructure.Persistence;

namespace Vendora.Infrastructure.Repositories;

public class ProductAuditLogRepository(VendoraDbContext context) : IProductAuditLogRepository
{
    public async Task<ProductAuditLog?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.ProductAuditLogs.FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task<IReadOnlyList<ProductAuditLog>> GetAllAsync(CancellationToken ct = default) =>
        await context.ProductAuditLogs.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<ProductAuditLog>> GetByProductIdAsync(Guid productId, CancellationToken ct = default) =>
        await context.ProductAuditLogs.AsNoTracking().Where(l => l.ProductId == productId).ToListAsync(ct);

    public async Task AddAsync(ProductAuditLog entity, CancellationToken ct = default) =>
        await context.ProductAuditLogs.AddAsync(entity, ct);

    public void Update(ProductAuditLog entity) => context.ProductAuditLogs.Update(entity);

    public void Remove(ProductAuditLog entity) => context.ProductAuditLogs.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
}
