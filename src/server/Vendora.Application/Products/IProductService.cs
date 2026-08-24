namespace Vendora.Application.Products;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken ct = default);
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<ProductDto?> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default);
    Task<ProductDto?> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default);
    Task<IReadOnlyList<ProductAuditLogDto>?> GetAuditLogAsync(Guid id, CancellationToken ct = default);
}
