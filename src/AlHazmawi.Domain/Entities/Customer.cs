using AlHazmawi.Domain.Common;
using AlHazmawi.Domain.Enums;

namespace AlHazmawi.Domain.Entities;

/// <summary>
/// Customer profile linked to an ASP.NET Identity User.
/// </summary>
public class Customer : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? FcmToken { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
}
