using AlHazmawi.Domain.Entities;
using AlHazmawi.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlHazmawi.Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly AlHazmawiDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        AlHazmawiDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            await SeedRolesAsync();
            await SeedAdminAsync();
            await SeedCategoriesAsync();
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        string[] roles = { "Admin", "Business", "Driver", "Customer" };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
                _logger.LogInformation("Role created: {Role}", role);
            }
        }
    }

    private async Task SeedAdminAsync()
    {
        var adminEmail = "admin@alhazmawi.com";
        var adminPhone = "0790000000";

        var adminUser = await _userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                PhoneNumber = adminPhone,
                FullName = "مسؤول النظام - AlHazmawi Admin",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(adminUser, "Admin@123456");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                _logger.LogInformation("Default Admin user created successfully.");
            }
            else
            {
                _logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private async Task SeedCategoriesAsync()
    {
        if (!await _context.BusinessCategories.AnyAsync())
        {
            var categories = new List<BusinessCategory>
            {
                new() { NameArabic = "مطاعم ووجبات سريعة", NameEnglish = "Restaurants & Fast Food", IconEmoji = "🍔", SortOrder = 1 },
                new() { NameArabic = "حلويات ومخابز", NameEnglish = "Sweets & Bakeries", IconEmoji = "🍰", SortOrder = 2 },
                new() { NameArabic = "سوبرماركت وبقالة", NameEnglish = "Supermarket & Groceries", IconEmoji = "🛒", SortOrder = 3 },
                new() { NameArabic = "خضار وفواكه", NameEnglish = "Fruits & Vegetables", IconEmoji = "🍎", SortOrder = 4 },
                new() { NameArabic = "صيدليات وعناية", NameEnglish = "Pharmacies & Care", IconEmoji = "💊", SortOrder = 5 },
                new() { NameArabic = "مشروبات ومقاهي", NameEnglish = "Cafes & Drinks", IconEmoji = "☕", SortOrder = 6 },
                new() { NameArabic = "زهور وهدايا", NameEnglish = "Flowers & Gifts", IconEmoji = "🎁", SortOrder = 7 },
            };

            await _context.BusinessCategories.AddRangeAsync(categories);
            _logger.LogInformation("Seeded {Count} initial business categories.", categories.Count);
        }
    }
}
