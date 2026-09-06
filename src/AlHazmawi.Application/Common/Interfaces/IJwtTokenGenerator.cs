namespace AlHazmawi.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateToken(string userId, string userName, string? email, string? phoneNumber, IList<string> roles);
    string GenerateRefreshToken();
}
