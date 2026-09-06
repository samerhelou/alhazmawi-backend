using AlHazmawi.Domain.Common;

namespace AlHazmawi.Domain.Entities;

/// <summary>
/// Product/item within a business. Supports any product type.
/// </summary>
public class Product : BaseEntity
{
    public Guid BusinessId { get; set; }
    public Guid? ProductCategoryId { get; set; }
    
    public string NameArabic { get; set; } = string.Empty;
    public string NameEnglish { get; set; } = string.Empty;
    public string? DescriptionArabic { get; set; }
    public string? DescriptionEnglish { get; set; }
    
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; } = true;
    public int SortOrder { get; set; } = 0;
    
    // Navigation
    public Business Business { get; set; } = null!;
    public ProductCategory? ProductCategory { get; set; }
    public ICollection<ProductOption> Options { get; set; } = new List<ProductOption>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
