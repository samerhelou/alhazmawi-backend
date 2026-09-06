using AlHazmawi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlHazmawi.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.NameArabic).IsRequired().HasMaxLength(200);
        builder.Property(p => p.NameEnglish).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Price).HasPrecision(10, 2);
        
        builder.HasIndex(p => p.BusinessId);
        builder.HasIndex(p => p.IsAvailable);
        
        builder.HasOne(p => p.Business)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(p => p.ProductCategory)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.ProductCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
