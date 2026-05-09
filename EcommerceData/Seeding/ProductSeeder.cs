using EcommerceData.Data;
using EcommerceData.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceData.Seeding;

public class ProductSeeder
{
    private readonly EcommerceDbContext _db;

    public ProductSeeder(EcommerceDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _db.Products.AnyAsync(cancellationToken))
        {
            return;
        }

        _db.Products.AddRange(
            new Product
            {
                Name = "Demo Smart Water Bottle",
                Description = "Tracks hydration and syncs with your phone.",
                Price = 49.99m
            },
            new Product
            {
                Name = "Another Smart Water Bottle",
                Description = "Tracks hydration and syncs with your phone.",
                Price = 49.99m
            });

        await _db.SaveChangesAsync(cancellationToken);
    }
}
