using AlHazmawi.Domain.Common;

namespace AlHazmawi.Domain.Entities;

/// <summary>
/// Customer favorite businesses list.
/// </summary>
public class Favorite : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Guid BusinessId { get; set; }
    
    // Navigation
    public Customer Customer { get; set; } = null!;
    public Business Business { get; set; } = null!;
}
