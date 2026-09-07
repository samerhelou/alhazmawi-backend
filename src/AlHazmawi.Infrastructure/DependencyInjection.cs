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
        // Render gives postgres:// URL - convert to Npgsql Host= format
        if (connStr.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) || connStr.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var uri = new Uri(connStr);
                var userInfo = uri.UserInfo.Split(':', 2);
                var user = Uri.UnescapeDataString(userInfo[0]);
                var pass = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
                var host = uri.Host;
                var port = uri.Port > 0 ? uri.Port : 5432;
                var db = uri.AbsolutePath.TrimStart('/');
                // Render requires SSL
                connStr = $"Host={host};Port={port};Database={db};Username={user};Password={pass};SslMode=Require;Trust Server Certificate=true";
            }
            catch { }
        }
        var isPostgres = connStr.Contains("Host=", StringComparison.OrdinalIgnoreCase);
        services.AddDbContext<AlHazmawiDbContext>(options =>
        {
            if (isPostgres)
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
