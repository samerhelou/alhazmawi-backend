using AlHazmawi.Domain.Common;

namespace AlHazmawi.Domain.Entities;

/// <summary>
/// Tracks coupon usage per customer to enforce per-user limits.
/// </summary>
public class CouponUsage : BaseEntity
{
    public Guid CouponId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid OrderId { get; set; }
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public Coupon Coupon { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
}
