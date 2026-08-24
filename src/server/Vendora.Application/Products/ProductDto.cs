namespace Vendora.Application.Products;

public record ProductDto(Guid Id, string Sku, string Name, string? Description, decimal Price, int QuantityOnHand, bool IsActive);

public record CreateProductRequest(string Sku, string Name, decimal Price, int InitialQuantity, string? Description);

public record UpdateProductRequest(string Name, decimal Price, string? Description);
