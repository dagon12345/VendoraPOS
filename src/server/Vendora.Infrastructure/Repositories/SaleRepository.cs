using Microsoft.EntityFrameworkCore;
using Vendora.Domain.Sales;
using Vendora.Infrastructure.Persistence;

namespace Vendora.Infrastructure.Repositories;

public class SaleRepository(VendoraDbContext context) : ISaleRepository
{
    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Sales.Include(s => s.Lines).FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<Sale>> GetAllAsync(CancellationToken ct = default) =>
        await context.Sales.Include(s => s.Lines).AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(Sale entity, CancellationToken ct = default) =>
        await context.Sales.AddAsync(entity, ct);

    public void Update(Sale entity) => context.Sales.Update(entity);

    public void Remove(Sale entity) => context.Sales.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
}
