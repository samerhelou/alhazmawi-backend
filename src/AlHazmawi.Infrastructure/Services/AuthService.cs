using AlHazmawi.Application.Common.Interfaces;
using AlHazmawi.Application.DTOs.Auth;
using AlHazmawi.Domain.Entities;
using AlHazmawi.Domain.Enums;
using AlHazmawi.Infrastructure.Identity;
using AlHazmawi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlHazmawi.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AlHazmawiDbContext _dbContext;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AlHazmawiDbContext dbContext,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var input = request.UsernameOrPhone.Trim();
        
        // Find user by phone, username, or email
        var user = await _userManager.Users.FirstOrDefaultAsync(u =>
            u.PhoneNumber == input ||
            u.UserName == input ||
            u.Email == input);

        if (user == null)
            return AuthResponse.Fail("Invalid credentials / رقم الهاتف أو كلمة المرور غير صحيحة");

        if (!user.IsActive)
            return AuthResponse.Fail("Account is deactivated / تم تعطيل هذا الحساب");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
            return AuthResponse.Fail("Invalid credentials / رقم الهاتف أو كلمة المرور غير صحيحة");

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? "Customer";

        // Get Profile Id according to role
        Guid? profileId = null;
        if (primaryRole == "Customer")
        {
            var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
            profileId = customer?.Id;
        }
        else if (primaryRole == "Business")
        {
            var business = await _dbContext.Businesses.FirstOrDefaultAsync(b => b.UserId == user.Id);
            profileId = business?.Id;
        }
        else if (primaryRole == "Driver")
        {
            var driver = await _dbContext.Drivers.FirstOrDefaultAsync(d => d.UserId == user.Id);
            profileId = driver?.Id;
        }

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(
            user.Id,
            user.UserName ?? user.PhoneNumber ?? user.Id,
            user.Email,
            user.PhoneNumber,
            roles);

        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(14);
        await _userManager.UpdateAsync(user);

        var userDto = new UserDto(
            user.Id,
            user.FullName,
            user.PhoneNumber ?? string.Empty,
            user.Email,
            primaryRole,
            profileId);

        return AuthResponse.Ok(token, refreshToken, expiresAt, primaryRole, userDto);
    }

    public async Task<AuthResponse> RegisterCustomerAsync(RegisterCustomerRequest request)
    {
        var phone = request.PhoneNumber.Trim();

        var existingUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone || u.UserName == phone);
        if (existingUser != null)
            return AuthResponse.Fail("Phone number is already registered / رقم الهاتف مسجل مسبقاً");

        var user = new ApplicationUser
        {
            UserName = phone,
            PhoneNumber = phone,
            FullName = request.FullName.Trim(),
            Email = request.Email?.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return AuthResponse.Fail(errors);
        }

        await EnsureRoleExistsAsync("Customer");
        await _userManager.AddToRoleAsync(user, "Customer");

        var customer = new Customer
        {
            UserId = user.Id,
            FullName = user.FullName,
            Phone = user.PhoneNumber,
            Email = user.Email,
            IsActive = true
        };

        if (!string.IsNullOrWhiteSpace(request.AddressTitle) || !string.IsNullOrWhiteSpace(request.Street))
        {
            customer.Addresses.Add(new Address
            {
                CustomerId = customer.Id,
                Label = request.AddressTitle ?? "Home",
                AddressLine = request.Street ?? "",
                Latitude = request.Latitude ?? 32.0,
                Longitude = request.Longitude ?? 35.0,
                IsDefault = true
            });
        }

        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(
            user.Id,
            user.UserName,
            user.Email,
            user.PhoneNumber,
            new[] { "Customer" });

        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(14);
        await _userManager.UpdateAsync(user);

        var userDto = new UserDto(
            user.Id,
            user.FullName,
            user.PhoneNumber,
            user.Email,
            "Customer",
            customer.Id);

        return AuthResponse.Ok(token, refreshToken, expiresAt, "Customer", userDto);
    }

    public async Task<AuthResponse> RegisterBusinessAsync(RegisterBusinessRequest request)
    {
        var phone = request.PhoneNumber.Trim();

        var existingUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone || u.UserName == phone);
        if (existingUser != null)
            return AuthResponse.Fail("Phone number is already registered / رقم الهاتف مسجل مسبقاً");

        var nameArNorm = request.NameAr.Trim().ToLower();
        var duplicateName = await _dbContext.Businesses.AnyAsync(b => b.NameArabic.ToLower() == nameArNorm);
        if (duplicateName)
            return AuthResponse.Fail("اسم المتجر موجود مسبقاً - اختر اسماً آخر / Business name already exists");

        var user = new ApplicationUser
        {
            UserName = phone,
            PhoneNumber = phone,
            FullName = request.NameAr.Trim(),
            Email = request.Email?.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return AuthResponse.Fail(errors);
        }

        await EnsureRoleExistsAsync("Business");
        await _userManager.AddToRoleAsync(user, "Business");

        var business = new Business
        {
            UserId = user.Id,
            CategoryId = request.CategoryId,
            NameArabic = request.NameAr.Trim(),
            NameEnglish = request.NameEn.Trim(),
            Phone = user.PhoneNumber,
            AddressArabic = request.Address ?? "",
            AddressEnglish = request.Address,
            Latitude = request.Latitude ?? 32.0,
            Longitude = request.Longitude ?? 35.0,
            Status = BusinessStatus.PendingApproval,
            IsActive = false,
            ApprovedAt = null,
            DeliveryFee = 2.0m,
            EstimatedDeliveryMinutes = 30
        };

        _dbContext.Businesses.Add(business);
        await _dbContext.SaveChangesAsync();

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(
            user.Id,
            user.UserName,
            user.Email,
            user.PhoneNumber,
            new[] { "Business" });

        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(14);
        await _userManager.UpdateAsync(user);

        var userDto = new UserDto(
            user.Id,
            user.FullName,
            user.PhoneNumber,
            user.Email,
            "Business",
            business.Id);

        return AuthResponse.Ok(token, refreshToken, expiresAt, "Business", userDto);
    }

    public async Task<AuthResponse> RegisterDriverAsync(RegisterDriverRequest request)
    {
        var phone = request.PhoneNumber.Trim();

        var existingUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone || u.UserName == phone);
        if (existingUser != null)
            return AuthResponse.Fail("Phone number is already registered / رقم الهاتف مسجل مسبقاً");

        var user = new ApplicationUser
        {
            UserName = phone,
            PhoneNumber = phone,
            FullName = request.FullName.Trim(),
            Email = request.Email?.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return AuthResponse.Fail(errors);
        }

        await EnsureRoleExistsAsync("Driver");
        await _userManager.AddToRoleAsync(user, "Driver");

        var driver = new Driver
        {
            UserId = user.Id,
            FullName = user.FullName,
            Phone = user.PhoneNumber,
            VehicleType = request.VehicleType,
            VehiclePlate = request.PlateNumber,
            Status = DriverStatus.Offline,
            IsActive = true
        };

        _dbContext.Drivers.Add(driver);
        await _dbContext.SaveChangesAsync();

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(
            user.Id,
            user.UserName,
            user.Email,
            user.PhoneNumber,
            new[] { "Driver" });

        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(14);
        await _userManager.UpdateAsync(user);

        var userDto = new UserDto(
            user.Id,
            user.FullName,
            user.PhoneNumber,
            user.Email,
            "Driver",
            driver.Id);

        return AuthResponse.Ok(token, refreshToken, expiresAt, "Driver", userDto);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);
        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return AuthResponse.Fail("Invalid or expired refresh token / رمز التحديث غير صالح أو منتهي الصلاحية");

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? "Customer";

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(
            user.Id,
            user.UserName ?? user.Id,
            user.Email,
            user.PhoneNumber,
            roles);

        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(14);
        await _userManager.UpdateAsync(user);

        var userDto = new UserDto(
            user.Id,
            user.FullName,
            user.PhoneNumber ?? "",
            user.Email,
            primaryRole);

        return AuthResponse.Ok(token, newRefreshToken, expiresAt, primaryRole, userDto);
    }

    public async Task<UserDto?> GetCurrentUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? "Customer";

        Guid? profileId = null;
        if (primaryRole == "Customer")
        {
            var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
            profileId = customer?.Id;
        }
        else if (primaryRole == "Business")
        {
            var business = await _dbContext.Businesses.FirstOrDefaultAsync(b => b.UserId == user.Id);
            profileId = business?.Id;
        }
        else if (primaryRole == "Driver")
        {
            var driver = await _dbContext.Drivers.FirstOrDefaultAsync(d => d.UserId == user.Id);
            profileId = driver?.Id;
        }

        return new UserDto(user.Id, user.FullName, user.PhoneNumber ?? "", user.Email, primaryRole, profileId);
    }

    private async Task EnsureRoleExistsAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
