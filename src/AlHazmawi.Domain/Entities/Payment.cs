using AlHazmawi.Domain.Common;
using AlHazmawi.Domain.Enums;

namespace AlHazmawi.Domain.Entities;

/// <summary>
/// Payment record for an order.
/// NEVER stores card numbers or CVV.
/// </summary>
public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public decimal Amount { get; set; }
    
    // External payment provider reference (for online payments)
    public string? ExternalTransactionId { get; set; }
    public string? ProviderName { get; set; }
    
    public DateTime? PaidAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    
    // Navigation
    public Order Order { get; set; } = null!;
}
