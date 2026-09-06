using AlHazmawi.Domain.Common;
using AlHazmawi.Domain.Enums;

namespace AlHazmawi.Domain.Entities;

/// <summary>
/// Delivery record linked to an order, tracking the driver's assignment and progress.
/// </summary>
public class Delivery : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid DriverId { get; set; }
    
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    
    public bool IsCompleted { get; set; } = false;
    
    // Navigation
    public Order Order { get; set; } = null!;
    public Driver Driver { get; set; } = null!;
    public ICollection<DriverLocation> LocationHistory { get; set; } = new List<DriverLocation>();
}
