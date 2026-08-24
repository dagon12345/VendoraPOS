using Vendora.Domain.Common;

namespace Vendora.Domain.Products;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);
}
