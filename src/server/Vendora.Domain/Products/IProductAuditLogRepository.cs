using Vendora.Domain.Common;

namespace Vendora.Domain.Products;

public interface IProductAuditLogRepository : IRepository<ProductAuditLog>
{
    Task<IReadOnlyList<ProductAuditLog>> GetByProductIdAsync(Guid productId, CancellationToken ct = default);
}
