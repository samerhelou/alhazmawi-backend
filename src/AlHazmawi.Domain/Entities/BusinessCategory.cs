using AlHazmawi.Domain.Common;

namespace AlHazmawi.Domain.Entities;

/// <summary>
/// Business/store category (e.g., Restaurant, Pharmacy, Supermarket).
/// Admin-managed. Not hard-coded.
/// </summary>
public class BusinessCategory : BaseEntity
{
    public string NameArabic { get; set; } = string.Empty;
    public string NameEnglish { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public string? IconEmoji { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    
    // Navigation
    public ICollection<Business> Businesses { get; set; } = new List<Business>();
}
