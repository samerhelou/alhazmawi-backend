using AlHazmawi.Domain.Common;
using AlHazmawi.Domain.Enums;

namespace AlHazmawi.Domain.Entities;

/// <summary>
/// Tracks every status change of an order with actor information.
/// </summary>
public class OrderStatusHistory : BaseEntity
{
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public string? ActorId { get; set; } // UserId who triggered the change
    public string? ActorRole { get; set; } // Customer, Business, Driver, Admin
    public string? Note { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public Order Order { get; set; } = null!;
}
