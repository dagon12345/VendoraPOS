using Microsoft.EntityFrameworkCore;
using Vendora.Domain.StockMovements;
using Vendora.Infrastructure.Persistence;

namespace Vendora.Infrastructure.Repositories;

public class StockMovementRepository(VendoraDbContext context) : IStockMovementRepository
{
    public async Task<StockMovement?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.StockMovements.FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<IReadOnlyList<StockMovement>> GetAllAsync(CancellationToken ct = default) =>
        await context.StockMovements.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<StockMovement>> GetByProductIdAsync(Guid productId, CancellationToken ct = default) =>
        await context.StockMovements.AsNoTracking().Where(m => m.ProductId == productId).ToListAsync(ct);

    public async Task AddAsync(StockMovement entity, CancellationToken ct = default) =>
        await context.StockMovements.AddAsync(entity, ct);

    public void Update(StockMovement entity) => context.StockMovements.Update(entity);

    public void Remove(StockMovement entity) => context.StockMovements.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
}
