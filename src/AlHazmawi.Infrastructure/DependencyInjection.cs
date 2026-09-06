using AlHazmawi.Infrastructure.Identity;
using AlHazmawi.Infrastructure.Persistence;
using AlHazmawi.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlHazmawi.Infrastructure;

/// <summary>
/// Infrastructure layer dependency injection registration.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database - auto-detect SqlServer vs PostgreSQL (for free cloud deploy)
        var connStr = configuration.GetConnectionString("DefaultConnection") ?? "";
        services.AddDbContext<AlHazmawiDbContext>(options =>
        {
            if (connStr.Contains("Host=", StringComparison.OrdinalIgnoreCase) || connStr.Contains("Username=", StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(connStr, b => b.MigrationsAssembly(typeof(AlHazmawiDbContext).Assembly.FullName));
            }
            else
            {
                options.UseSqlServer(connStr, b => b.MigrationsAssembly(typeof(AlHazmawiDbContext).Assembly.FullName));
            }
        });
        
        services.AddScoped<AlHazmawi.Application.Common.Interfaces.IApplicationDbContext>(provider =>
            provider.GetRequiredService<AlHazmawiDbContext>());
        
        // Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.SignIn.RequireConfirmedEmail = false;
            options.User.RequireUniqueEmail = false; // Phone is primary
        })
        .AddEntityFrameworkStores<AlHazmawiDbContext>()
        .AddDefaultTokenProviders();
        
        // Services
        services.AddScoped<IStorageService, LocalStorageService>();
        services.AddScoped<AlHazmawi.Application.Common.Interfaces.IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<AlHazmawi.Application.Common.Interfaces.IAuthService, AuthService>();
        services.AddScoped<AlHazmawi.Application.Common.Interfaces.ICategoryService, CategoryService>();
        services.AddScoped<AlHazmawi.Application.Common.Interfaces.IBusinessService, BusinessService>();
        services.AddScoped<AlHazmawi.Application.Common.Interfaces.IProductService, ProductService>();
        services.AddScoped<AlHazmawi.Application.Common.Interfaces.IOrderService, OrderService>();
        services.AddScoped<DatabaseSeeder>();
        
        return services;
    }
}
