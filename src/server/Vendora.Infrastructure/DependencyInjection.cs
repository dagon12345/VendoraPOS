using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vendora.Application.Products;
using Vendora.Application.StockMovements;
using Vendora.Domain.Products;
using Vendora.Domain.StockMovements;
using Vendora.Infrastructure.Persistence;
using Vendora.Infrastructure.Repositories;

namespace Vendora.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<VendoraDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductAuditLogRepository, ProductAuditLogRepository>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IStockMovementRepository, StockMovementRepository>();
        services.AddScoped<IStockMovementService, StockMovementService>();

        return services;
    }
}
