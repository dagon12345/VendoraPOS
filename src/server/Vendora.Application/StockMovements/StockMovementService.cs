using Vendora.Domain.Products;
using Vendora.Domain.StockMovements;

namespace Vendora.Application.StockMovements;

public class StockMovementService(IStockMovementRepository movementRepository, IProductRepository productRepository) : IStockMovementService
{
    public async Task<IReadOnlyList<StockMovementDto>?> GetHistoryAsync(Guid productId, CancellationToken ct = default)
    {
        var product = await productRepository.GetByIdAsync(productId, ct);
        if (product is null) return null;

        var movements = await movementRepository.GetByProductIdAsync(productId, ct);
        return movements.OrderByDescending(m => m.CreatedAtUtc).Select(ToDto).ToList();
    }

    public async Task<StockMovementDto?> RecordAsync(Guid productId, RecordStockMovementRequest request, CancellationToken ct = default)
    {
        if (request.Reason == StockMovementReason.InitialStock)
            throw new ArgumentException("InitialStock is recorded automatically and cannot be set manually.", nameof(request));

        var product = await productRepository.GetByIdAsync(productId, ct);
        if (product is null) return null;

        product.AdjustStock(request.QuantityDelta);
        productRepository.Update(product);

        var movement = StockMovement.Create(productId, request.QuantityDelta, request.Reason, request.Note);
        await movementRepository.AddAsync(movement, ct);

        await movementRepository.SaveChangesAsync(ct);
        return ToDto(movement);
    }

    private static StockMovementDto ToDto(StockMovement m) =>
        new(m.Id, m.ProductId, m.QuantityDelta, m.Reason, m.Note, m.CreatedAtUtc);
}
