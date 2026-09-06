namespace AlHazmawi.Application.DTOs.Auth;

public record LoginRequest(
    string UsernameOrPhone,
    string Password
);

public record RegisterCustomerRequest(
    string FullName,
    string PhoneNumber,
    string Password,
    string? Email = null,
    string? AddressTitle = null,
    string? Street = null,
    double? Latitude = null,
    double? Longitude = null
);

public record RegisterBusinessRequest(
    string NameAr,
    string NameEn,
    Guid CategoryId,
    string PhoneNumber,
    string Password,
    string? Email = null,
    string? Address = null,
    string? CommercialRegistrationNo = null,
    double? Latitude = null,
    double? Longitude = null
);

public record RegisterDriverRequest(
    string FullName,
    string PhoneNumber,
    string Password,
    string? Email = null,
    string VehicleType = "Motorcycle",
    string PlateNumber = "",
    string NationalId = ""
);

public record UserDto(
    string Id,
    string FullName,
    string PhoneNumber,
    string? Email,
    string Role,
    Guid? ProfileId = null
);

public record AuthResponse(
    bool Success,
    string? Token = null,
    string? RefreshToken = null,
    DateTime? ExpiresAt = null,
    string? Role = null,
    UserDto? User = null,
    List<string>? Errors = null
)
{
    public static AuthResponse Fail(string error) =>
        new(false, Errors: new List<string> { error });

    public static AuthResponse Fail(List<string> errors) =>
        new(false, Errors: errors);

    public static AuthResponse Ok(string token, string refreshToken, DateTime expiresAt, string role, UserDto user) =>
        new(true, Token: token, RefreshToken: refreshToken, ExpiresAt: expiresAt, Role: role, User: user);
}

public record RefreshTokenRequest(
    string Token,
    string RefreshToken
);
