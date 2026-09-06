using AlHazmawi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlHazmawi.Infrastructure.Persistence.Configurations;

public class BusinessConfiguration : IEntityTypeConfiguration<Business>
{
    public void Configure(EntityTypeBuilder<Business> builder)
    {
        builder.HasKey(b => b.Id);
        
        builder.Property(b => b.NameArabic).IsRequired().HasMaxLength(200);
        builder.Property(b => b.NameEnglish).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Phone).IsRequired().HasMaxLength(20);
        builder.Property(b => b.AddressArabic).IsRequired().HasMaxLength(500);
        builder.Property(b => b.DeliveryFee).HasPrecision(10, 2);
        builder.Property(b => b.MinimumOrderAmount).HasPrecision(10, 2);
        builder.Property(b => b.AverageRating).HasPrecision(3, 2);
        
        builder.HasIndex(b => b.Status);
        builder.HasIndex(b => b.CategoryId);
        builder.HasIndex(b => b.IsActive);
        
        builder.HasOne(b => b.Category)
            .WithMany(c => c.Businesses)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
