using AlHazmawi.Domain.Common;

namespace AlHazmawi.Domain.Entities;

/// <summary>
/// Snapshot of a product at order time. Preserves historical price.
/// </summary>
public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    
    public string ProductNameArabic { get; set; } = string.Empty;
    public string ProductNameEnglish { get; set; } = string.Empty;
    
    // Historical unit price at time of order — server fetches from DB
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; } // UnitPrice * Quantity
    
    public string? Notes { get; set; }
    public string? SelectedOptions { get; set; } // JSON snapshot of selected options
    
    // Navigation
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
