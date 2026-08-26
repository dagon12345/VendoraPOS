using Vendora.Domain.Products;

namespace Vendora.Application.Products;

public record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    decimal Price,
    int QuantityOnHand,
    bool IsActive,
    string? Barcode,
    string? ImageUrl,
    DateOnly? ExpiryDate);

public record CreateProductRequest(
    string Sku,
    string Name,
    decimal Price,
    int InitialQuantity,
    string? Description,
    string? Barcode = null,
    string? ImageUrl = null,
    DateOnly? ExpiryDate = null);

public record UpdateProductRequest(
    string Name,
    decimal Price,
    string? Description,
    string? Barcode = null,
    string? ImageUrl = null,
    DateOnly? ExpiryDate = null);

public record ProductAuditLogDto(Guid Id, Guid ProductId, ProductAuditAction Action, string Summary, DateTime CreatedAtUtc);
