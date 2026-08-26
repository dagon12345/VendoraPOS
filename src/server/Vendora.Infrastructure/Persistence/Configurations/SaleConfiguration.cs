using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vendora.Domain.Sales;

namespace Vendora.Infrastructure.Persistence.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.PaymentMethod).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(s => s.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(s => s.AmountTendered).HasColumnType("decimal(18,2)");
        builder.Property(s => s.ChangeDue).HasColumnType("decimal(18,2)");
        builder.Property(s => s.VoidReason).HasMaxLength(500);

        builder.HasMany(s => s.Lines)
            .WithOne()
            .HasForeignKey("SaleId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Sale.Lines))!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
