using AlHazmawi.Domain.Common;

namespace AlHazmawi.Domain.Entities;

/// <summary>
/// Geographic service area. Admin-managed. Not hard-coded.
/// Initial area: Hizma + surrounding Jerusalem suburbs.
/// </summary>
public class ServiceArea : BaseEntity
{
    public string NameArabic { get; set; } = string.Empty;
    public string NameEnglish { get; set; } = string.Empty;
    
    // Center coordinates
    public double CenterLatitude { get; set; }
    public double CenterLongitude { get; set; }
    
    // Radius in kilometers
    public double RadiusKm { get; set; }
    
    public bool IsActive { get; set; } = true;
}
