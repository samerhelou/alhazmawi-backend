using Microsoft.AspNetCore.Identity;

namespace AlHazmawi.Infrastructure.Identity;

/// <summary>
/// Extended Identity User with platform-specific fields.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? FcmToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
