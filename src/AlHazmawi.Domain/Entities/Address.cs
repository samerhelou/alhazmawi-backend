using AlHazmawi.Domain.Common;

namespace AlHazmawi.Domain.Entities;

/// <summary>
/// Customer delivery address with GPS coordinates.
/// </summary>
public class Address : BaseEntity
{
    public Guid CustomerId { get; set; }
    public string Label { get; set; } = string.Empty; // e.g., "Home", "Work"
    public string AddressLine { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsDefault { get; set; } = false;
    
    // Navigation
    public Customer Customer { get; set; } = null!;
}
