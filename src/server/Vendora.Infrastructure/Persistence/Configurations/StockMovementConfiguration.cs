using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vendora.Domain.Products;
using Vendora.Domain.StockMovements;

namespace Vendora.Infrastructure.Persistence.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.ProductId).IsRequired();
        builder.HasIndex(m => m.ProductId);

        builder.Property(m => m.Reason).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(m => m.Note).HasMaxLength(1000);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
