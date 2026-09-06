using AlHazmawi.Domain.Common;

namespace AlHazmawi.Domain.Entities;

/// <summary>
/// Customer review for a business after a completed order.
/// Customers rate businesses only — not drivers.
/// Rating: 1-5 stars.
/// </summary>
public class Review : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Guid BusinessId { get; set; }
    public Guid OrderId { get; set; }
    
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
    public bool IsVisible { get; set; } = true;
    
    // Navigation
    public Customer Customer { get; set; } = null!;
    public Business Business { get; set; } = null!;
    public Order Order { get; set; } = null!;
}
