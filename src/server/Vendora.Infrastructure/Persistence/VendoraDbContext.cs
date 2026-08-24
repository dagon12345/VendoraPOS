using Microsoft.EntityFrameworkCore;
using Vendora.Domain.Products;
using Vendora.Domain.StockMovements;

namespace Vendora.Infrastructure.Persistence;

public class VendoraDbContext(DbContextOptions<VendoraDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<ProductAuditLog> ProductAuditLogs => Set<ProductAuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VendoraDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
