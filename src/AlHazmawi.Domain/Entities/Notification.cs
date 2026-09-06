using AlHazmawi.Domain.Common;
using AlHazmawi.Domain.Enums;

namespace AlHazmawi.Domain.Entities;

/// <summary>
/// Customer notification. Supports both in-app display and FCM push delivery.
/// </summary>
public class Notification : BaseEntity
{
    public Guid CustomerId { get; set; }
    public NotificationType Type { get; set; }
    
    public string TitleArabic { get; set; } = string.Empty;
    public string TitleEnglish { get; set; } = string.Empty;
    public string BodyArabic { get; set; } = string.Empty;
    public string BodyEnglish { get; set; } = string.Empty;
    
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    
    // Deep link data
    public Guid? RelatedOrderId { get; set; }
    
    // Navigation
    public Customer Customer { get; set; } = null!;
}
