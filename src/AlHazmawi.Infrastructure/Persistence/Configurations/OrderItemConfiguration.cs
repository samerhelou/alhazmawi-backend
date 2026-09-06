using AlHazmawi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlHazmawi.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(i => i.Id);
        
        builder.Property(i => i.UnitPrice).HasPrecision(10, 2);
        builder.Property(i => i.LineTotal).HasPrecision(10, 2);
        builder.Property(i => i.ProductNameArabic).IsRequired().HasMaxLength(200);
        builder.Property(i => i.ProductNameEnglish).IsRequired().HasMaxLength(200);
        
        builder.HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Keep product reference even if product is deleted (historical)
        builder.HasOne(i => i.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
