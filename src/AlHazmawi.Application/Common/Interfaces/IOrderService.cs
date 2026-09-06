using AlHazmawi.Application.DTOs.Orders;
using AlHazmawi.Domain.Enums;

namespace AlHazmawi.Application.Common.Interfaces;

public interface IOrderService
{
    Task<OrderDetailDto> CreateOrderAsync(CreateOrderRequest request, string userId);
    Task<OrderDetailDto?> GetOrderByIdAsync(Guid orderId, string userId, string role);
    Task<List<OrderSummaryDto>> GetCustomerOrdersAsync(string userId);
    Task<List<OrderSummaryDto>> GetBusinessOrdersAsync(Guid businessId, OrderStatus? status = null);
    Task<List<OrderSummaryDto>> GetDriverOrdersAsync(string userId, bool activeOnly = true);
    Task<List<OrderSummaryDto>> GetAvailableOrdersForDriversAsync(double? currentLat, double? currentLon);
    Task<bool> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request, string userId, string role);
    Task<bool> AssignDriverAsync(Guid orderId, Guid driverId, string userId);
}
