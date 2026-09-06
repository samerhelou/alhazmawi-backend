using AlHazmawi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlHazmawi.Infrastructure.Persistence.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Code).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Value).HasPrecision(10, 2);
        builder.Property(c => c.MinimumOrderAmount).HasPrecision(10, 2);
        builder.Property(c => c.MaximumDiscount).HasPrecision(10, 2);
        
        builder.HasIndex(c => c.Code).IsUnique();
        builder.HasIndex(c => c.IsActive);
    }
}
