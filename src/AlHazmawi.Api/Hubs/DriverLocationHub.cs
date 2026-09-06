using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AlHazmawi.Api.Hubs;

[Authorize]
public class DriverLocationHub : Hub
{
    public async Task JoinTracking(string orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"track_{orderId}");
    }

    public async Task LeaveTracking(string orderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"track_{orderId}");
    }

    [Authorize(Roles = "Driver")]
    public async Task SendLocation(string orderId, double latitude, double longitude)
    {
        await Clients.Group($"track_{orderId}").SendAsync("LocationUpdated", new
        {
            OrderId = orderId,
            Latitude = latitude,
            Longitude = longitude,
            Timestamp = DateTime.UtcNow
        });
    }
}
