using Microsoft.EntityFrameworkCore;
using Vendora.Domain.Products;
using Vendora.Domain.Sales;
using Vendora.Domain.StockMovements;

namespace Vendora.Infrastructure.Persistence;

public class VendoraDbContext(DbContextOptions<VendoraDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<ProductAuditLog> ProductAuditLogs => Set<ProductAuditLog>();
    public DbSet<Sale> Sales => Set<Sale>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VendoraDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Every DateTime column here is UTC by convention (the "...Utc" naming) - see
        // UtcDateTimeConverter for why this is needed.
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }
}
