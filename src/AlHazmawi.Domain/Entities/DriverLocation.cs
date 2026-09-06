using AlHazmawi.Domain.Common;

namespace AlHazmawi.Domain.Entities;

/// <summary>
/// Driver GPS location snapshot during active delivery.
/// Only recorded during active delivery to avoid unnecessary data accumulation.
/// </summary>
public class DriverLocation : BaseEntity
{
    public Guid DriverId { get; set; }
    public Guid? DeliveryId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public Driver Driver { get; set; } = null!;
    public Delivery? Delivery { get; set; }
}
