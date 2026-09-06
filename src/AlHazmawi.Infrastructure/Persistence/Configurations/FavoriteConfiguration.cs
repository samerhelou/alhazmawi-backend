using AlHazmawi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlHazmawi.Infrastructure.Persistence.Configurations;

public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.HasKey(f => f.Id);
        
        // One favorite per customer per business
        builder.HasIndex(f => new { f.CustomerId, f.BusinessId }).IsUnique();
        
        builder.HasOne(f => f.Customer)
            .WithMany(c => c.Favorites)
            .HasForeignKey(f => f.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(f => f.Business)
            .WithMany(b => b.Favorites)
            .HasForeignKey(f => f.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
