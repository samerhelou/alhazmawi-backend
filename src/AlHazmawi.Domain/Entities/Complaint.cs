using AlHazmawi.Domain.Common;
using AlHazmawi.Domain.Enums;

namespace AlHazmawi.Domain.Entities;

/// <summary>
/// Customer complaint about an order.
/// Admin can view and manage complaints.
/// </summary>
public class Complaint : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Guid OrderId { get; set; }
    
    public string Category { get; set; } = string.Empty; // missing_item, wrong_item, delivery_problem, etc.
    public string Description { get; set; } = string.Empty;
    
    public ComplaintStatus Status { get; set; } = ComplaintStatus.Open;
    public string? AdminNote { get; set; }
    public DateTime? ResolvedAt { get; set; }
    
    // Navigation
    public Customer Customer { get; set; } = null!;
    public Order Order { get; set; } = null!;
}
