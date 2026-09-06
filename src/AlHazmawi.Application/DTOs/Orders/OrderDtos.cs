using AlHazmawi.Domain.Enums;

namespace AlHazmawi.Application.DTOs.Orders;

public record CreateOrderItemRequest(
    Guid ProductId,
    int Quantity,
    List<Guid>? SelectedOptionIds = null,
    string? Notes = null
);

public record CreateOrderRequest(
    Guid BusinessId,
    Guid? AddressId = null,
    string? CustomAddressLine = null,
    double? DeliveryLatitude = null,
    double? DeliveryLongitude = null,
    string? DeliveryPhone = null,
    string? CustomerNote = null,
    PaymentMethod PaymentMethod = PaymentMethod.CashOnDelivery,
    string? CouponCode = null,
    List<CreateOrderItemRequest>? Items = null
);

public record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductNameArabic,
    string ProductNameEnglish,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal,
    string? Notes,
    string? SelectedOptionsJson
);

public record OrderStatusHistoryDto(
    OrderStatus Status,
    string? Note,
    DateTime ChangedAt,
    string? ActorRole
);

public record OrderSummaryDto(
    Guid Id,
    string OrderNumber,
    Guid BusinessId,
    string BusinessNameArabic,
    string BusinessNameEnglish,
    string? BusinessLogoUrl,
    OrderStatus Status,
    decimal Total,
    DateTime CreatedAt,
    int ItemCount
);

public record OrderDetailDto(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    string CustomerName,
    string CustomerPhone,
    Guid BusinessId,
    string BusinessNameArabic,
    string BusinessNameEnglish,
    string BusinessPhone,
    Guid? DriverId,
    string? DriverName,
    string? DriverPhone,
    string DeliveryAddressLine,
    double DeliveryLatitude,
    double DeliveryLongitude,
    string DeliveryPhone,
    decimal Subtotal,
    decimal DeliveryFee,
    decimal Discount,
    decimal Total,
    PaymentMethod PaymentMethod,
    PaymentStatus PaymentStatus,
    OrderStatus Status,
    string? CustomerNote,
    string? RejectionReason,
    DateTime CreatedAt,
    List<OrderItemDto> Items,
    List<OrderStatusHistoryDto> StatusHistory
);

public record UpdateOrderStatusRequest(
    OrderStatus NewStatus,
    string? Note = null,
    string? RejectionReason = null
);

public record AssignDriverRequest(
    Guid DriverId
);
