using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Vendora.Api.Hubs;
using Vendora.Api.Realtime;
using Vendora.Application.Common.Interfaces;
using Vendora.Application.Products;
using Vendora.Infrastructure;
using Vendora.Infrastructure.Persistence;

const string AngularDevCorsPolicy = "AngularDev";

// Must happen before WebApplication.CreateBuilder - ASP.NET Core's static web assets loader
// crashes at builder-creation time if wwwroot doesn't exist at all yet (e.g. a fresh clone that
// has never received an upload), so the folder needs to be there before the host even starts.
// The content root defaults to the current directory (true for `dotnet run`/`dotnet watch`).
Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products"));

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);

// Registered here rather than in Vendora.Infrastructure's AddInfrastructure - StockHub is a
// web-layer type Infrastructure can't reference (wrong dependency direction), so its implementation
// lives in Api, wired up at the composition root like everything else in this file.
builder.Services.AddSignalR();
builder.Services.AddScoped<IStockNotifier, SignalRStockNotifier>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularDevCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseCors(AngularDevCorsPolicy);

    using var seedScope = app.Services.CreateScope();
    var dbContext = seedScope.ServiceProvider.GetRequiredService<VendoraDbContext>();
    await dbContext.Database.MigrateAsync();
    var productService = seedScope.ServiceProvider.GetRequiredService<IProductService>();
    await DbSeeder.SeedAsync(dbContext, productService);
}

app.UseHttpsRedirection();

// Serves uploaded product images from wwwroot/uploads back out at /uploads/... . Now that
// wwwroot is guaranteed to exist before the builder was created (see above), the environment's
// auto-detected file provider resolves correctly.
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();
app.MapHub<StockHub>("/hubs/stock");

app.Run();
