using Microsoft.AspNetCore.SignalR;
using Vendora.Application.Common.Interfaces;

namespace Vendora.Api.Hubs;

public class StockHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, IStockNotifier.DefaultScope);
        await base.OnConnectedAsync();
    }
}
