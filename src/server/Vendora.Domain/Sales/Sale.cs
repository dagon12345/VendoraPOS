using Vendora.Domain.Common;

namespace Vendora.Domain.Sales;

public record SaleLineInput(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);

/// <summary>
/// A completed checkout transaction. Aggregate root owning its line items. Immutable once
/// created except for <see cref="Void"/> — a sale is never deleted or edited, matching the
/// immutable-ledger philosophy already used for StockMovement/ProductAuditLog.
/// </summary>
public class Sale : BaseEntity
{
    private readonly List<SaleLine> _lines = [];

    public PaymentMethod PaymentMethod { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal? AmountTendered { get; private set; }
    public decimal? ChangeDue { get; private set; }
    public bool IsVoided { get; private set; }
    public DateTime? VoidedAtUtc { get; private set; }
    public string? VoidReason { get; private set; }
    public IReadOnlyList<SaleLine> Lines => _lines;

    private Sale() { }

    public static Sale Create(IReadOnlyList<SaleLineInput> lines, PaymentMethod paymentMethod, decimal? amountTendered)
    {
        if (lines is not { Count: > 0 }) throw new ArgumentException("A sale must have at least one line.", nameof(lines));

        var sale = new Sale { PaymentMethod = paymentMethod };

        foreach (var line in lines)
        {
            sale._lines.Add(SaleLine.Create(line.ProductId, line.ProductName, line.UnitPrice, line.Quantity));
        }

        sale.TotalAmount = sale._lines.Sum(l => l.UnitPrice * l.Quantity);

        if (paymentMethod == PaymentMethod.Cash)
        {
            if (amountTendered is null || amountTendered < sale.TotalAmount)
                throw new ArgumentException("Amount tendered must cover the total for a cash sale.", nameof(amountTendered));

            sale.AmountTendered = amountTendered;
            sale.ChangeDue = amountTendered - sale.TotalAmount;
        }

        return sale;
    }

    public void Void(string? reason)
    {
        if (IsVoided) throw new InvalidOperationException("Sale is already voided.");

        IsVoided = true;
        VoidedAtUtc = DateTime.UtcNow;
        VoidReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        MarkUpdated();
    }

    /// <summary>Undoes a mistaken Void, restoring the sale to active and re-deducting stock (see
    /// SaleService.RestoreAsync). Not a general "undo checkout" - only reverses a Void.</summary>
    public void Restore()
    {
        if (!IsVoided) throw new InvalidOperationException("Sale is not voided.");

        IsVoided = false;
        VoidedAtUtc = null;
        VoidReason = null;
        MarkUpdated();
    }

    /// <summary>Returns/voids part of one line (e.g. a customer returning one product out of a
    /// larger order) without touching the rest of the sale - a partial refund, distinct from
    /// <see cref="Void"/> which cancels the whole transaction at once. Restocking and the
    /// compensating StockMovement are handled by SaleService.VoidLineAsync.</summary>
    public void VoidLine(Guid productId, int quantity)
    {
        if (IsVoided) throw new InvalidOperationException("Sale is already fully voided - nothing left to void per-line.");

        var line = _lines.FirstOrDefault(l => l.ProductId == productId)
            ?? throw new ArgumentException("This sale has no line for that product.", nameof(productId));

        line.VoidQuantity(quantity);
        MarkUpdated();
    }

    /// <summary>Undoes a mistaken per-line return - the line-level counterpart to
    /// <see cref="Restore"/>. Blocked while the whole sale is voided, since that's a different
    /// state to undo via <see cref="Restore"/> instead.</summary>
    public void RestoreLine(Guid productId, int quantity)
    {
        if (IsVoided) throw new InvalidOperationException("Sale is fully voided - restore the whole sale instead.");

        var line = _lines.FirstOrDefault(l => l.ProductId == productId)
            ?? throw new ArgumentException("This sale has no line for that product.", nameof(productId));

        line.RestoreQuantity(quantity);
        MarkUpdated();
    }
}
