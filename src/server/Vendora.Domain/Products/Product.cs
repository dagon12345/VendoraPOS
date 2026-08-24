using Vendora.Domain.Common;

namespace Vendora.Domain.Products;

/// <summary>
/// A sellable item. Deliberately generic (no pharmacy/retail/cafe-specific fields)
/// so the same aggregate serves every vertical; per-vertical data (e.g. drug batch/expiry)
/// will attach via a separate bounded-context extension once that module is built.
/// </summary>
public class Product : BaseEntity
{
    public string Sku { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int QuantityOnHand { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Product() { }

    public static Product Create(string sku, string name, decimal price, int initialQuantity = 0, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentException("SKU is required.", nameof(sku));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        if (initialQuantity < 0) throw new ArgumentOutOfRangeException(nameof(initialQuantity), "Quantity cannot be negative.");

        return new Product
        {
            Sku = sku.Trim(),
            Name = name.Trim(),
            Description = description,
            Price = price,
            QuantityOnHand = initialQuantity,
        };
    }

    public void UpdateDetails(string name, decimal price, string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");

        Name = name.Trim();
        Price = price;
        Description = description;
        MarkUpdated();
    }

    public void AdjustStock(int delta)
    {
        var newQuantity = QuantityOnHand + delta;
        if (newQuantity < 0) throw new InvalidOperationException("Stock cannot go negative.");

        QuantityOnHand = newQuantity;
        MarkUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkUpdated();
    }
}
