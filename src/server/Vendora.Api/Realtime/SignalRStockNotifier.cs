using Microsoft.AspNetCore.SignalR;
using Vendora.Api.Hubs;
using Vendora.Application.Common.Interfaces;

namespace Vendora.Api.Realtime;

public class SignalRStockNotifier(IHubContext<StockHub> hubContext) : IStockNotifier
{
    public Task NotifyStockChangedAsync(Guid productId, int quantityOnHand, string storeScope = IStockNotifier.DefaultScope, CancellationToken ct = default) =>
        hubContext.Clients.Group(storeScope).SendAsync("StockChanged", new { productId, quantityOnHand }, ct);
}
