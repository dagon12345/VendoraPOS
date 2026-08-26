using Vendora.Domain.Common;

namespace Vendora.Domain.Sales;

/// <summary>
/// One line item of a completed sale. Snapshots the product's name and price at the moment of
/// sale, so the record stays accurate even if the product is later renamed, repriced, or
/// deactivated. Only ever created via <see cref="Sale.Create"/>.
/// </summary>
public class SaleLine : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    /// <summary>How many units of this line have been individually returned/voided (a partial
    /// refund) - independent of the whole-sale <see cref="Sale.IsVoided"/> flag. See
    /// <see cref="Sale.VoidLine"/>.</summary>
    public int VoidedQuantity { get; private set; }

    /// <summary>Units of this line still counted as sold - what a partial-void reduces.</summary>
    public int ActiveQuantity => Quantity - VoidedQuantity;

    private SaleLine() { }

    internal static SaleLine Create(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (productId == Guid.Empty) throw new ArgumentException("ProductId is required.", nameof(productId));
        if (string.IsNullOrWhiteSpace(productName)) throw new ArgumentException("Product name is required.", nameof(productName));
        if (unitPrice < 0) throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative.");
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

        return new SaleLine
        {
            ProductId = productId,
            ProductName = productName,
            UnitPrice = unitPrice,
            Quantity = quantity,
        };
    }

    internal void VoidQuantity(int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        if (quantity > ActiveQuantity)
            throw new InvalidOperationException($"Cannot void {quantity} - only {ActiveQuantity} of this line is still active.");

        VoidedQuantity += quantity;
    }

    internal void RestoreQuantity(int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        if (quantity > VoidedQuantity)
            throw new InvalidOperationException($"Cannot restore {quantity} - only {VoidedQuantity} of this line has been returned.");

        VoidedQuantity -= quantity;
    }
}
