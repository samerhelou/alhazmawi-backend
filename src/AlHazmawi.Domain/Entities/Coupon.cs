using AlHazmawi.Domain.Common;
using AlHazmawi.Domain.Enums;

namespace AlHazmawi.Domain.Entities;

/// <summary>
/// Discount coupon created by Admin only.
/// Supports percentage, fixed amount, and free delivery.
/// </summary>
public class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public CouponType Type { get; set; }
    
    public decimal Value { get; set; } // Percentage (0-100) or fixed amount
    
    public DateTime StartDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaximumDiscount { get; set; } // Cap for percentage coupons
    
    public int? UsageLimit { get; set; } // null = unlimited
    public int UsageCount { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation
    public ICollection<CouponUsage> Usages { get; set; } = new List<CouponUsage>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
