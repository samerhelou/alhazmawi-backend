using AlHazmawi.Application.DTOs.Auth;

namespace AlHazmawi.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterCustomerAsync(RegisterCustomerRequest request);
    Task<AuthResponse> RegisterBusinessAsync(RegisterBusinessRequest request);
    Task<AuthResponse> RegisterDriverAsync(RegisterDriverRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task<UserDto?> GetCurrentUserAsync(string userId);
}
