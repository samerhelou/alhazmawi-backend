using AlHazmawi.Domain.Common;

namespace AlHazmawi.Domain.Entities;

/// <summary>
/// Business working hours per day of week.
/// Each business defines its own schedule.
/// </summary>
public class BusinessWorkingHours : BaseEntity
{
    public Guid BusinessId { get; set; }
    
    // 0=Sunday, 1=Monday, ..., 6=Saturday
    public DayOfWeek DayOfWeek { get; set; }
    
    public bool IsOpen { get; set; } = true;
    public TimeOnly? OpeningTime { get; set; }
    public TimeOnly? ClosingTime { get; set; }
    
    // Navigation
    public Business Business { get; set; } = null!;
}
