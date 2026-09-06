using AlHazmawi.Domain.Common;

namespace AlHazmawi.Domain.Entities;

/// <summary>
/// Represents a business/store registered on the platform.
/// Can be a restaurant, pharmacy, supermarket, shop, etc.
/// </summary>
public class Business : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    
    public string NameArabic { get; set; } = string.Empty;
    public string NameEnglish { get; set; } = string.Empty;
    public string? DescriptionArabic { get; set; }
    public string? DescriptionEnglish { get; set; }
    
    public string Phone { get; set; } = string.Empty;
    public string AddressArabic { get; set; } = string.Empty;
    public string? AddressEnglish { get; set; }
    
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    public string? LogoUrl { get; set; }
    public string? CoverUrl { get; set; }
    
    public decimal DeliveryFee { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public int EstimatedDeliveryMinutes { get; set; } = 30;
    
    public Enums.BusinessStatus Status { get; set; } = Enums.BusinessStatus.PendingApproval;
    public bool IsActive { get; set; } = false;
    
    public string? RejectionReason { get; set; }
    public DateTime? ApprovedAt { get; set; }
    
    // Computed rating stored as denormalized field for performance
    public double AverageRating { get; set; } = 0;
    public int ReviewCount { get; set; } = 0;
    
    // Navigation properties
    public BusinessCategory Category { get; set; } = null!;
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<BusinessWorkingHours> WorkingHours { get; set; } = new List<BusinessWorkingHours>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}
