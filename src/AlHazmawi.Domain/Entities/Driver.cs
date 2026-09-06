using AlHazmawi.Domain.Common;
using AlHazmawi.Domain.Enums;

namespace AlHazmawi.Domain.Entities;

/// <summary>
/// Driver profile linked to an ASP.NET Identity User.
/// </summary>
public class Driver : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public string? VehicleType { get; set; }
    public string? VehiclePlate { get; set; }
    public string? FcmToken { get; set; }
    
    public DriverStatus Status { get; set; } = DriverStatus.Offline;
    public bool IsActive { get; set; } = true;
    
    // Current location (updated via SignalR during active delivery)
    public double? CurrentLatitude { get; set; }
    public double? CurrentLongitude { get; set; }
    public DateTime? LastLocationUpdate { get; set; }
    
    // Navigation
    public ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
    public ICollection<DriverLocation> LocationHistory { get; set; } = new List<DriverLocation>();
}
