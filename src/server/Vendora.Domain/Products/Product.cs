using Vendora.Domain.Common;

namespace Vendora.Domain.Products;

/// <summary>
/// A sellable item. Deliberately generic (no pharmacy/retail/cafe-specific fields) so the same
/// aggregate serves every vertical - ExpiryDate is optional and simply left blank by stores that
/// don't need it. A dedicated batch/lot-tracking extension (multiple expiry dates per SKU) would
/// still attach separately if that level of detail is ever needed.
/// </summary>
public class Product : BaseEntity
{
    public string Sku { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int QuantityOnHand { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? Barcode { get; private set; }
    public string? ImageUrl { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }

    private Product() { }

    public static Product Create(
        string sku,
        string name,
        decimal price,
        int initialQuantity = 0,
        string? description = null,
        string? barcode = null,
        string? imageUrl = null,
        DateOnly? expiryDate = null)
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
            Barcode = string.IsNullOrWhiteSpace(barcode) ? null : barcode.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim(),
            ExpiryDate = expiryDate,
        };
    }

    public void UpdateDetails(string name, decimal price, string? description, string? barcode, string? imageUrl, DateOnly? expiryDate)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");

        Name = name.Trim();
        Price = price;
        Description = description;
        Barcode = string.IsNullOrWhiteSpace(barcode) ? null : barcode.Trim();
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
        ExpiryDate = expiryDate;
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
