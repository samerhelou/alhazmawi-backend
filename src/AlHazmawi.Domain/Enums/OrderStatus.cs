namespace AlHazmawi.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,
    Accepted = 2,
    Preparing = 3,
    DriverAssigned = 4,
    PickedUp = 5,
    OnTheWay = 6,
    Delivered = 7,
    Cancelled = 8,
    Rejected = 9
}
