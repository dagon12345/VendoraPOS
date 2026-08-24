using Vendora.Domain.Products;

namespace Vendora.Application.Products;

public class ProductService(IProductRepository repository) : IProductService
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
        await repository.SaveChangesAsync(ct);
        return ToDto(product);
    }

    public async Task<ProductDto?> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default)
    {
        var product = await repository.GetByIdAsync(id, ct);
        if (product is null) return null;

        product.UpdateDetails(request.Name, request.Price, request.Description);
        repository.Update(product);
        await repository.SaveChangesAsync(ct);
        return ToDto(product);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await repository.GetByIdAsync(id, ct);
        if (product is null) return false;

        repository.Remove(product);
        await repository.SaveChangesAsync(ct);
        return true;
    }

    private static ProductDto ToDto(Product p) =>
        new(p.Id, p.Sku, p.Name, p.Description, p.Price, p.QuantityOnHand, p.IsActive);
}
