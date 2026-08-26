namespace Vendora.Application.Common.Interfaces;

public interface IStockNotifier
{
    const string DefaultScope = "default";

    Task NotifyStockChangedAsync(Guid productId, int quantityOnHand, string storeScope = DefaultScope, CancellationToken ct = default);
}
