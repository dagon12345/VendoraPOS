using Microsoft.EntityFrameworkCore;
using Vendora.Domain.Products;

namespace Vendora.Infrastructure.Persistence;

public class VendoraDbContext(DbContextOptions<VendoraDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VendoraDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
