using Microsoft.EntityFrameworkCore;
using Vendora.Application.Products;

namespace Vendora.Infrastructure.Persistence;

/// <summary>
/// Seeds sample products (spanning pharmacy, retail, and coffee-shop use cases, matching this
/// project's general-purpose data model) into an empty database, so cloning the repo and running
/// migrations gives every contributor the same starting data instead of an empty product list.
/// Goes through <see cref="IProductService"/> rather than raw EF inserts, so seeded products get
/// proper domain validation and an InitialStock movement, exactly like a real created product.
/// </summary>
public static class DbSeeder
{
    private static readonly (string Sku, string Name, decimal Price, int Quantity, string Description, string Barcode)[] SampleProducts =
    [
        ("RX-001", "Paracetamol 500mg", 5.50m, 100, "Pain reliever", "8901234500001"),
        ("RX-002", "Amoxicillin 500mg", 12.75m, 60, "Antibiotic capsule", "8901234500002"),
        ("RX-003", "Cetirizine 10mg", 8.25m, 80, "Antihistamine tablet", "8901234500003"),
        ("RX-004", "Ibuprofen 200mg", 6.00m, 120, "Anti-inflammatory tablet", "8901234500004"),
        ("RX-005", "Vitamin C 500mg", 4.50m, 150, "Immune support supplement", "8901234500005"),
        ("RX-006", "Cough Syrup 120ml", 9.99m, 45, "Cough relief syrup", "8901234500006"),
        ("RT-001", "AA Batteries (4-pack)", 3.25m, 200, "Alkaline batteries", "8901234500007"),
        ("RT-002", "LED Light Bulb 9W", 2.75m, 150, "Energy-saving bulb", "8901234500008"),
        ("RT-003", "Notebook A5", 1.99m, 300, "Ruled notebook", "8901234500009"),
        ("RT-004", "Ballpoint Pen (Blue)", 0.50m, 500, "Standard ballpoint pen", "8901234500010"),
        ("RT-005", "USB Flash Drive 32GB", 7.99m, 90, "USB 3.0 flash drive", "8901234500011"),
        ("RT-006", "Phone Charger Cable", 5.99m, 110, "USB-C charging cable", "8901234500012"),
        ("RT-007", "Hand Sanitizer 250ml", 3.50m, 130, "Antibacterial gel", "8901234500013"),
        ("RT-008", "Reusable Shopping Bag", 1.25m, 200, "Eco-friendly tote bag", "8901234500014"),
        ("CF-001", "Espresso Beans 250g", 8.50m, 40, "Whole bean espresso roast", "8901234500015"),
        ("CF-002", "Paper Cup 12oz (Sleeve of 50)", 4.25m, 60, "Disposable coffee cups", "8901234500016"),
        ("CF-003", "Oat Milk 1L", 3.75m, 35, "Barista blend oat milk", "8901234500017"),
        ("CF-004", "Vanilla Syrup 750ml", 6.50m, 25, "Flavored coffee syrup", "8901234500018"),
        ("CF-005", "Chocolate Muffin", 2.50m, 50, "Freshly baked muffin", "8901234500019"),
        ("CF-006", "Iced Tea Bottle 500ml", 2.25m, 70, "Bottled iced tea", "8901234500020"),
    ];

    public static async Task SeedAsync(VendoraDbContext context, IProductService productService, CancellationToken ct = default)
    {
        if (await context.Products.AnyAsync(ct))
        {
            return;
        }

        foreach (var sample in SampleProducts)
        {
            await productService.CreateAsync(
                new CreateProductRequest(sample.Sku, sample.Name, sample.Price, sample.Quantity, sample.Description, sample.Barcode), ct);
        }
    }
}
