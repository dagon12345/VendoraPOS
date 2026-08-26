using Vendora.Application.Common.Interfaces;
using Vendora.Domain.Products;
using Vendora.Domain.Sales;
using Vendora.Domain.StockMovements;

namespace Vendora.Application.Sales;

public class SaleService(
    ISaleRepository saleRepository,
    IProductRepository productRepository,
    IStockMovementRepository movementRepository,
    IStockNotifier stockNotifier) : ISaleService
{
    public async Task<SaleDto> CreateSaleAsync(CreateSaleRequest request, CancellationToken ct = default)
    {
        var lineInputs = new List<SaleLineInput>();
        var stockChanges = new List<(Guid ProductId, int QuantityOnHand)>();

        foreach (var line in request.Lines)
        {
            var product = await productRepository.GetByIdAsync(line.ProductId, ct);
            if (product is null)
                throw new ArgumentException($"Product {line.ProductId} not found.", nameof(request));
            if (!product.IsActive)
                throw new InvalidOperationException($"'{product.Name}' is inactive and cannot be sold.");

            lineInputs.Add(new SaleLineInput(product.Id, product.Name, product.Price, line.Quantity));

            product.AdjustStock(-line.Quantity);
            productRepository.Update(product);
            stockChanges.Add((product.Id, product.QuantityOnHand));
        }

        var sale = Sale.Create(lineInputs, request.PaymentMethod, request.AmountTendered);
        await saleRepository.AddAsync(sale, ct);

        foreach (var line in sale.Lines)
        {
            var movement = StockMovement.Create(line.ProductId, -line.Quantity, StockMovementReason.Sale, $"Sale {sale.Id}");
            await movementRepository.AddAsync(movement, ct);
        }

        await saleRepository.SaveChangesAsync(ct);

        foreach (var (productId, quantityOnHand) in stockChanges)
            await stockNotifier.NotifyStockChangedAsync(productId, quantityOnHand, ct: ct);

        return ToDto(sale);
    }

    public async Task<SaleDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var sale = await saleRepository.GetByIdAsync(id, ct);
        return sale is null ? null : ToDto(sale);
    }

    public async Task<IReadOnlyList<SaleDto>> GetAllAsync(CancellationToken ct = default)
    {
        var sales = await saleRepository.GetAllAsync(ct);
        return sales.OrderByDescending(s => s.CreatedAtUtc).Select(ToDto).ToList();
    }

    public async Task<SaleDto?> VoidAsync(Guid id, VoidSaleRequest request, CancellationToken ct = default)
    {
        var sale = await saleRepository.GetByIdAsync(id, ct);
        if (sale is null) return null;

        sale.Void(request.Reason);
        saleRepository.Update(sale);

        var stockChanges = new List<(Guid ProductId, int QuantityOnHand)>();

        foreach (var line in sale.Lines)
        {
            // Restock only what's still active - a line already partially returned via VoidLine
            // shouldn't have its returned units restocked a second time here.
            if (line.ActiveQuantity == 0) continue;

            var product = await productRepository.GetByIdAsync(line.ProductId, ct);
            if (product is null) continue;

            product.AdjustStock(line.ActiveQuantity);
            productRepository.Update(product);
            stockChanges.Add((product.Id, product.QuantityOnHand));

            var movement = StockMovement.Create(line.ProductId, line.ActiveQuantity, StockMovementReason.Sale, $"Void of sale {sale.Id}");
            await movementRepository.AddAsync(movement, ct);
        }

        await saleRepository.SaveChangesAsync(ct);

        foreach (var (productId, quantityOnHand) in stockChanges)
            await stockNotifier.NotifyStockChangedAsync(productId, quantityOnHand, ct: ct);

        return ToDto(sale);
    }

    /// <summary>Undoes a mistaken Void - re-deducts stock (throwing if it's no longer available,
    /// e.g. sold to someone else since the void) and flips the sale back to active, atomically.</summary>
    public async Task<SaleDto?> RestoreAsync(Guid id, CancellationToken ct = default)
    {
        var sale = await saleRepository.GetByIdAsync(id, ct);
        if (sale is null) return null;

        sale.Restore();
        saleRepository.Update(sale);

        var stockChanges = new List<(Guid ProductId, int QuantityOnHand)>();

        foreach (var line in sale.Lines)
        {
            if (line.ActiveQuantity == 0) continue;

            var product = await productRepository.GetByIdAsync(line.ProductId, ct);
            if (product is null) continue;

            product.AdjustStock(-line.ActiveQuantity);
            productRepository.Update(product);
            stockChanges.Add((product.Id, product.QuantityOnHand));

            var movement = StockMovement.Create(line.ProductId, -line.ActiveQuantity, StockMovementReason.Sale, $"Restore of sale {sale.Id}");
            await movementRepository.AddAsync(movement, ct);
        }

        await saleRepository.SaveChangesAsync(ct);

        foreach (var (productId, quantityOnHand) in stockChanges)
            await stockNotifier.NotifyStockChangedAsync(productId, quantityOnHand, ct: ct);

        return ToDto(sale);
    }

    /// <summary>Voids/returns part of one line - a partial refund - without touching the rest of
    /// the sale. Restocks exactly the returned quantity and records a compensating StockMovement,
    /// same pattern as the whole-sale Void/Restore.</summary>
    public async Task<SaleDto?> VoidLineAsync(Guid id, VoidLineRequest request, CancellationToken ct = default)
    {
        var sale = await saleRepository.GetByIdAsync(id, ct);
        if (sale is null) return null;

        sale.VoidLine(request.ProductId, request.Quantity);
        saleRepository.Update(sale);

        var product = await productRepository.GetByIdAsync(request.ProductId, ct);
        if (product is not null)
        {
            product.AdjustStock(request.Quantity);
            productRepository.Update(product);

            var note = string.IsNullOrWhiteSpace(request.Reason)
                ? $"Partial return ({request.Quantity}) from sale {sale.Id}"
                : $"Partial return ({request.Quantity}) from sale {sale.Id}: {request.Reason}";
            var movement = StockMovement.Create(request.ProductId, request.Quantity, StockMovementReason.Sale, note);
            await movementRepository.AddAsync(movement, ct);
        }

        await saleRepository.SaveChangesAsync(ct);

        if (product is not null)
            await stockNotifier.NotifyStockChangedAsync(product.Id, product.QuantityOnHand, ct: ct);

        return ToDto(sale);
    }

    /// <summary>Undoes a mistaken partial return - re-deducts the stock and reduces the line's
    /// VoidedQuantity back down, the line-level counterpart to RestoreAsync.</summary>
    public async Task<SaleDto?> RestoreLineAsync(Guid id, RestoreLineRequest request, CancellationToken ct = default)
    {
        var sale = await saleRepository.GetByIdAsync(id, ct);
        if (sale is null) return null;

        sale.RestoreLine(request.ProductId, request.Quantity);
        saleRepository.Update(sale);

        var product = await productRepository.GetByIdAsync(request.ProductId, ct);
        if (product is not null)
        {
            product.AdjustStock(-request.Quantity);
            productRepository.Update(product);

            var note = string.IsNullOrWhiteSpace(request.Reason)
                ? $"Restore of partial return ({request.Quantity}) from sale {sale.Id}"
                : $"Restore of partial return ({request.Quantity}) from sale {sale.Id}: {request.Reason}";
            var movement = StockMovement.Create(request.ProductId, -request.Quantity, StockMovementReason.Sale, note);
            await movementRepository.AddAsync(movement, ct);
        }

        await saleRepository.SaveChangesAsync(ct);

        if (product is not null)
            await stockNotifier.NotifyStockChangedAsync(product.Id, product.QuantityOnHand, ct: ct);

        return ToDto(sale);
    }

    private static SaleDto ToDto(Sale sale)
    {
        var lines = sale.Lines.Select(l => new SaleLineDto(
            l.ProductId, l.ProductName, l.UnitPrice, l.Quantity, l.UnitPrice * l.Quantity,
            l.VoidedQuantity, l.ActiveQuantity, l.UnitPrice * l.ActiveQuantity)).ToList();

        var refundedAmount = lines.Sum(l => l.UnitPrice * l.VoidedQuantity);

        return new SaleDto(
            sale.Id,
            sale.CreatedAtUtc,
            sale.PaymentMethod,
            sale.TotalAmount,
            sale.AmountTendered,
            sale.ChangeDue,
            sale.IsVoided,
            sale.VoidedAtUtc,
            sale.VoidReason,
            refundedAmount,
            sale.TotalAmount - refundedAmount,
            lines);
    }
}
