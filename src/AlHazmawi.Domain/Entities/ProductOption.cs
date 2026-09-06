using AlHazmawi.Domain.Common;

namespace AlHazmawi.Domain.Entities;

/// <summary>
/// A product option/add-on (e.g., size: Small/Medium/Large, extras: cheese, etc.)
/// </summary>
public class ProductOption : BaseEntity
{
    public Guid ProductId { get; set; }
    public string NameArabic { get; set; } = string.Empty;
    public string NameEnglish { get; set; } = string.Empty;
    public decimal AdditionalPrice { get; set; } = 0;
    public bool IsRequired { get; set; } = false;
    public bool IsActive { get; set; } = true;
    
    // Navigation
    public Product Product { get; set; } = null!;
}
