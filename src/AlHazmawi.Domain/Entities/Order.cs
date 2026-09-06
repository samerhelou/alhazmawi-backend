using AlHazmawi.Domain.Common;
using AlHazmawi.Domain.Enums;

namespace AlHazmawi.Domain.Entities;

/// <summary>
/// Core order entity. Server ALWAYS calculates subtotal, delivery fee, discount, and total.
/// Client-submitted totals are IGNORED.
/// </summary>
public class Order : BaseEntity
{
    // Visible order number: HZ-YYYYMMDD-XXXX
    public string OrderNumber { get; set; } = string.Empty;
    
    public Guid CustomerId { get; set; }
    public Guid BusinessId { get; set; }
    public Guid? DriverId { get; set; }
    
    // Delivery address (snapshot at order time)
    public string DeliveryAddressLine { get; set; } = string.Empty;
    public string? DeliveryAddressNotes { get; set; }
    public double DeliveryLatitude { get; set; }
    public double DeliveryLongitude { get; set; }
    public string DeliveryPhone { get; set; } = string.Empty;
    
    // Financials — ALL calculated server-side
    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Discount { get; set; } = 0;
    public decimal Total { get; set; }
    
    // Coupon
    public Guid? CouponId { get; set; }
    public string? CouponCode { get; set; }
    
    // Payment
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    
    // Order state
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    // Optional customer note
    public string? CustomerNote { get; set; }
    
    // Business rejection reason
    public string? RejectionReason { get; set; }
    
    // Timestamps
    public DateTime? AcceptedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    
    // Navigation
    public Customer Customer { get; set; } = null!;
    public Business Business { get; set; } = null!;
    public Driver? Driver { get; set; }
    public Coupon? Coupon { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    public Payment? Payment { get; set; }
    public Delivery? Delivery { get; set; }
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    public Review? Review { get; set; }
}
