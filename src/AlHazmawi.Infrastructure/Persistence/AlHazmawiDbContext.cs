using AlHazmawi.Application.Common.Interfaces;
using AlHazmawi.Domain.Entities;
using AlHazmawi.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AlHazmawi.Infrastructure.Persistence;

/// <summary>
/// Main EF Core database context for Al-Hazmawi Delivery.
/// Uses ASP.NET Core Identity for user management.
/// </summary>
public class AlHazmawiDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public AlHazmawiDbContext(DbContextOptions<AlHazmawiDbContext> options) : base(options)
    {
    }
    
    // Core entities
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<BusinessCategory> BusinessCategories => Set<BusinessCategory>();
    public DbSet<BusinessWorkingHours> BusinessWorkingHours => Set<BusinessWorkingHours>();
    public DbSet<Driver> Drivers => Set<Driver>();
    
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductOption> ProductOptions => Set<ProductOption>();
    
    public DbSet<Address> Addresses => Set<Address>();
    
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<DriverLocation> DriverLocations => Set<DriverLocation>();
    
    public DbSet<Payment> Payments => Set<Payment>();
    
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<CouponUsage> CouponUsages => Set<CouponUsage>();
    
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Complaint> Complaints => Set<Complaint>();
    
    public DbSet<ServiceArea> ServiceAreas => Set<ServiceArea>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AlHazmawiDbContext).Assembly);
        
        // Rename Identity tables to clean names
        modelBuilder.Entity<ApplicationUser>().ToTable("Users");

        // Global configuration for decimal columns
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("decimal(18,2)");
        }
    }
    
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }
    
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<Domain.Common.BaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
