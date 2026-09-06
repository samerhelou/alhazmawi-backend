namespace AlHazmawi.Domain.Enums;

public enum NotificationType
{
    OrderCreated = 1,
    OrderAccepted = 2,
    OrderRejected = 3,
    OrderPreparing = 4,
    DriverAssigned = 5,
    DriverPickedUp = 6,
    OrderOnTheWay = 7,
    OrderDelivered = 8,
    OrderCancelled = 9,
    NewOrder = 10,
    BusinessApproved = 11,
    BusinessRejected = 12,
    DeliveryAssigned = 13,
    General = 14
}
