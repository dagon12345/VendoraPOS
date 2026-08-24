using Vendora.Domain.Common;

namespace Vendora.Domain.StockMovements;

public interface IStockMovementRepository : IRepository<StockMovement>
{
    Task<IReadOnlyList<StockMovement>> GetByProductIdAsync(Guid productId, CancellationToken ct = default);
}
