using AlHazmawi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlHazmawi.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Customer> Customers { get; }
    DbSet<Business> Businesses { get; }
    DbSet<BusinessCategory> BusinessCategories { get; }
    DbSet<BusinessWorkingHours> BusinessWorkingHours { get; }
    DbSet<Driver> Drivers { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductCategory> ProductCategories { get; }
    DbSet<ProductOption> ProductOptions { get; }
    DbSet<Address> Addresses { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<OrderStatusHistory> OrderStatusHistories { get; }
    DbSet<Delivery> Deliveries { get; }
    DbSet<DriverLocation> DriverLocations { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Review> Reviews { get; }
    DbSet<Favorite> Favorites { get; }
    DbSet<Coupon> Coupons { get; }
    DbSet<CouponUsage> CouponUsages { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<Complaint> Complaints { get; }
    DbSet<ServiceArea> ServiceAreas { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
