using AlHazmawi.Domain.Enums;

namespace AlHazmawi.Application.Common.Interfaces;

public interface IOrderNotificationService
{
    Task NotifyOrderStatusChangedAsync(Guid orderId, Guid customerId, OrderStatus newStatus, string? note);
    Task NotifyNewOrderToBusinessAsync(Guid businessId, Guid orderId, string orderNumber);
    Task NotifyDriverAssignedAsync(Guid driverId, Guid orderId);
}
