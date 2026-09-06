using AlHazmawi.Api.Hubs;
using AlHazmawi.Application.Common.Interfaces;
using AlHazmawi.Domain.Enums;
using Microsoft.AspNetCore.SignalR;

namespace AlHazmawi.Api.Services;

public class OrderNotificationService : IOrderNotificationService
{
    private readonly IHubContext<OrderHub> _hubContext;
    private readonly ILogger<OrderNotificationService> _logger;

    public OrderNotificationService(
        IHubContext<OrderHub> hubContext,
        ILogger<OrderNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyOrderStatusChangedAsync(Guid orderId, Guid customerId, OrderStatus newStatus, string? note)
    {
        try
        {
            var payload = new
            {
                OrderId = orderId,
                Status = newStatus.ToString(),
                StatusCode = (int)newStatus,
                Note = note,
                Timestamp = DateTime.UtcNow
            };

            // Notify specific order room
            await _hubContext.Clients.Group($"order_{orderId}").SendAsync("OrderStatusChanged", payload);

            // Notify customer room
            await _hubContext.Clients.Group($"customer_{customerId}").SendAsync("OrderStatusChanged", payload);

            _logger.LogInformation("Notified order {OrderId} status changed to {Status}", orderId, newStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR notification for order {OrderId}", orderId);
        }
    }

    public async Task NotifyNewOrderToBusinessAsync(Guid businessId, Guid orderId, string orderNumber)
    {
        try
        {
            var payload = new
            {
                OrderId = orderId,
                OrderNumber = orderNumber,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group($"business_{businessId}").SendAsync("NewOrder", payload);
            _logger.LogInformation("Notified business {BusinessId} of new order {OrderNumber}", businessId, orderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR new order notification to business {BusinessId}", businessId);
        }
    }

    public async Task NotifyDriverAssignedAsync(Guid driverId, Guid orderId)
    {
        try
        {
            var payload = new
            {
                OrderId = orderId,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group($"driver_{driverId}").SendAsync("OrderAssigned", payload);
            _logger.LogInformation("Notified driver {DriverId} of assignment to order {OrderId}", driverId, orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR assignment notification to driver {DriverId}", driverId);
        }
    }
}
