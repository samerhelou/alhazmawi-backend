using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AlHazmawi.Api.Hubs;

[Authorize]
public class OrderHub : Hub
{
    public async Task JoinOrderGroup(string orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
    }

    public async Task LeaveOrderGroup(string orderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order_{orderId}");
    }

    public async Task JoinBusinessGroup(string businessId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"business_{businessId}");
    }

    public async Task JoinCustomerGroup(string customerId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"customer_{customerId}");
    }
}
