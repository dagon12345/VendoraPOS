using Vendora.Domain.Products;
using Vendora.Domain.StockMovements;

namespace Vendora.Application.Products;

public class ProductService(
    IProductRepository repository,
    IStockMovementRepository movementRepository,
    IProductAuditLogRepository auditLogRepository) : IProductService
{
    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken ct = default)
    {
        var products = await repository.GetAllAsync(ct);
        return products.Select(ToDto).ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = await repository.GetByIdAsync(id, ct);
        return product is null ? null : ToDto(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        var product = Product.Create(request.Sku, request.Name, request.Price, request.InitialQuantity, request.Description);
        await repository.AddAsync(product, ct);

        if (request.InitialQuantity > 0)
        {
            var movement = StockMovement.Create(product.Id, request.InitialQuantity, StockMovementReason.InitialStock);
            await movementRepository.AddAsync(movement, ct);
        }

        await repository.SaveChangesAsync(ct);
        return ToDto(product);
    }

    public async Task<ProductDto?> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default)
    {
        var product = await repository.GetByIdAsync(id, ct);
        if (product is null) return null;

        var changes = new List<string>();
        if (product.Name != request.Name) changes.Add($"Name: '{product.Name}' -> '{request.Name}'");
        if (product.Price != request.Price) changes.Add($"Price: {product.Price:0.00} -> {request.Price:0.00}");
        if (product.Description != request.Description) changes.Add($"Description: '{product.Description}' -> '{request.Description}'");

        product.UpdateDetails(request.Name, request.Price, request.Description);
        repository.Update(product);

        if (changes.Count > 0)
        {
            var log = ProductAuditLog.Create(product.Id, ProductAuditAction.Edited, string.Join("; ", changes));
            await auditLogRepository.AddAsync(log, ct);
        }

        await repository.SaveChangesAsync(ct);
        return ToDto(product);
    }

    public async Task<ProductDto?> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
    {
        var product = await repository.GetByIdAsync(id, ct);
        if (product is null) return null;

        if (isActive) product.Activate(); else product.Deactivate();
        repository.Update(product);

        var log = ProductAuditLog.Create(
            product.Id,
            isActive ? ProductAuditAction.Activated : ProductAuditAction.Deactivated,
            isActive ? "Product activated" : "Product deactivated");
        await auditLogRepository.AddAsync(log, ct);

        await repository.SaveChangesAsync(ct);
        return ToDto(product);
    }

    public async Task<IReadOnlyList<ProductAuditLogDto>?> GetAuditLogAsync(Guid id, CancellationToken ct = default)
    {
        var product = await repository.GetByIdAsync(id, ct);
        if (product is null) return null;

        var logs = await auditLogRepository.GetByProductIdAsync(id, ct);
        return logs.OrderByDescending(l => l.CreatedAtUtc)
            .Select(l => new ProductAuditLogDto(l.Id, l.ProductId, l.Action, l.Summary, l.CreatedAtUtc))
            .ToList();
    }

    private static ProductDto ToDto(Product p) =>
        new(p.Id, p.Sku, p.Name, p.Description, p.Price, p.QuantityOnHand, p.IsActive);
}
