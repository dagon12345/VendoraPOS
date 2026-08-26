using Vendora.Domain.Sales;

namespace Vendora.Application.Sales;

public record SaleLineDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal,
    int VoidedQuantity,
    int ActiveQuantity,
    decimal ActiveLineTotal);

public record SaleDto(
    Guid Id,
    DateTime CreatedAtUtc,
    PaymentMethod PaymentMethod,
    decimal TotalAmount,
    decimal? AmountTendered,
    decimal? ChangeDue,
    bool IsVoided,
    DateTime? VoidedAtUtc,
    string? VoidReason,
    decimal RefundedAmount,
    decimal NetTotal,
    IReadOnlyList<SaleLineDto> Lines);

public record SaleLineRequest(Guid ProductId, int Quantity);

public record CreateSaleRequest(IReadOnlyList<SaleLineRequest> Lines, PaymentMethod PaymentMethod, decimal? AmountTendered);

public record VoidSaleRequest(string? Reason);

/// <summary>A partial refund - voids just one product's line (or part of its quantity) within a
/// sale, leaving the rest of the transaction untouched. See Sale.VoidLine.</summary>
public record VoidLineRequest(Guid ProductId, int Quantity, string? Reason);

/// <summary>Undoes a mistaken partial return - the line-level counterpart to VoidSaleRequest's
/// whole-sale Restore. See Sale.RestoreLine.</summary>
public record RestoreLineRequest(Guid ProductId, int Quantity, string? Reason);
