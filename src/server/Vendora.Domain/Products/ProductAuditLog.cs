using Vendora.Domain.Common;

namespace Vendora.Domain.Products;

/// <summary>
/// An immutable log entry recording a change made to a product (details edited, or
/// activated/deactivated), so the change history is visible after the fact.
/// </summary>
public class ProductAuditLog : BaseEntity
{
    public Guid ProductId { get; private set; }
    public ProductAuditAction Action { get; private set; }
    public string Summary { get; private set; } = default!;

    private ProductAuditLog() { }

    public static ProductAuditLog Create(Guid productId, ProductAuditAction action, string summary)
    {
        if (productId == Guid.Empty) throw new ArgumentException("ProductId is required.", nameof(productId));
        if (string.IsNullOrWhiteSpace(summary)) throw new ArgumentException("Summary is required.", nameof(summary));

        return new ProductAuditLog
        {
            ProductId = productId,
            Action = action,
            Summary = summary.Trim(),
        };
    }
}
