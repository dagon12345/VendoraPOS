using Vendora.Domain.StockMovements;

namespace Vendora.Application.StockMovements;

public record StockMovementDto(Guid Id, Guid ProductId, int QuantityDelta, StockMovementReason Reason, string? Note, DateTime CreatedAtUtc);

public record RecordStockMovementRequest(int QuantityDelta, StockMovementReason Reason, string? Note);
