namespace Vendora.Application.Sales;

public interface ISaleService
{
    Task<SaleDto> CreateSaleAsync(CreateSaleRequest request, CancellationToken ct = default);
    Task<SaleDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<SaleDto>> GetAllAsync(CancellationToken ct = default);
    Task<SaleDto?> VoidAsync(Guid id, VoidSaleRequest request, CancellationToken ct = default);
    Task<SaleDto?> RestoreAsync(Guid id, CancellationToken ct = default);
    Task<SaleDto?> VoidLineAsync(Guid id, VoidLineRequest request, CancellationToken ct = default);
    Task<SaleDto?> RestoreLineAsync(Guid id, RestoreLineRequest request, CancellationToken ct = default);
}
