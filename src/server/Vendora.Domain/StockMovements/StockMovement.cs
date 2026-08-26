using Vendora.Domain.Common;

namespace Vendora.Domain.StockMovements;

/// <summary>
/// An immutable audit-trail entry recording why and by how much a product's stock changed.
/// References Product by ID only, deliberately kept as a separate aggregate so the ledger
/// survives independently of how Product itself evolves.
/// </summary>
public class StockMovement : BaseEntity
{
    public Guid ProductId { get; private set; }
    public int QuantityDelta { get; private set; }
    public StockMovementReason Reason { get; private set; }
    public string? Note { get; private set; }

    private StockMovement() { }

    public static StockMovement Create(Guid productId, int quantityDelta, StockMovementReason reason, string? note = null)
    {
        if (productId == Guid.Empty) throw new ArgumentException("ProductId is required.", nameof(productId));
        if (quantityDelta == 0) throw new ArgumentException("Quantity delta cannot be zero.", nameof(quantityDelta));

        // Restock/InitialStock always add stock; Waste always removes it. Adjustment (a manual
        // correction) and Sale (negative when sold, positive when a sale is voided) can go either way.
        if (reason is StockMovementReason.Restock or StockMovementReason.InitialStock && quantityDelta < 0)
            throw new ArgumentException($"{reason} must increase stock (quantity must be positive).", nameof(quantityDelta));
        if (reason == StockMovementReason.Waste && quantityDelta > 0)
            throw new ArgumentException("Waste must decrease stock (quantity must be negative).", nameof(quantityDelta));

        return new StockMovement
        {
            ProductId = productId,
            QuantityDelta = quantityDelta,
            Reason = reason,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
        };
    }
}
