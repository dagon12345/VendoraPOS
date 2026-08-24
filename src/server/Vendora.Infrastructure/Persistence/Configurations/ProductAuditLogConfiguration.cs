using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vendora.Domain.Products;

namespace Vendora.Infrastructure.Persistence.Configurations;

public class ProductAuditLogConfiguration : IEntityTypeConfiguration<ProductAuditLog>
{
    public void Configure(EntityTypeBuilder<ProductAuditLog> builder)
    {
        builder.ToTable("ProductAuditLogs");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.ProductId).IsRequired();
        builder.HasIndex(l => l.ProductId);

        builder.Property(l => l.Action).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(l => l.Summary).HasMaxLength(1000).IsRequired();

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
