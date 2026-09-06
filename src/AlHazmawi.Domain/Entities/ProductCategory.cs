using AlHazmawi.Domain.Common;

namespace AlHazmawi.Domain.Entities;

/// <summary>
/// Product categories within a business (e.g., "Appetizers", "Main Course" for restaurant
/// or "Dairy", "Vegetables" for a supermarket).
/// </summary>
public class ProductCategory : BaseEntity
{
    public Guid BusinessId { get; set; }
    public string NameArabic { get; set; } = string.Empty;
    public string NameEnglish { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    
    // Navigation
    public Business Business { get; set; } = null!;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
