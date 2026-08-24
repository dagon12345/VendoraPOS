namespace Vendora.Application.StockMovements;

public interface IStockMovementService
{
    Task<IReadOnlyList<StockMovementDto>?> GetHistoryAsync(Guid productId, CancellationToken ct = default);
    Task<StockMovementDto?> RecordAsync(Guid productId, RecordStockMovementRequest request, CancellationToken ct = default);
}
