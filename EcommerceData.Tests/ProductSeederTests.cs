using EcommerceData.Entities;
using EcommerceData.Seeding;
using Xunit;

namespace EcommerceData.Tests;

public class ProductSeederTests
{
    [Fact]
    public async Task SeedAsync_adds_products_when_database_is_empty()
    {
        using var fixture = new SqliteDbContextFixture();
        var seeder = new ProductSeeder(fixture.DbContext);

        await seeder.SeedAsync();

        Assert.NotEmpty(fixture.DbContext.Products);
    }

    [Fact]
    public async Task SeedAsync_is_idempotent_when_products_already_exist()
    {
        using var fixture = new SqliteDbContextFixture();
        fixture.DbContext.Products.Add(new Product { Name = "Existing", Description = "x", Price = 1m });
        await fixture.DbContext.SaveChangesAsync();

        var seeder = new ProductSeeder(fixture.DbContext);
        await seeder.SeedAsync();

        Assert.Single(fixture.DbContext.Products);
        Assert.Equal("Existing", fixture.DbContext.Products.First().Name);
    }

    [Fact]
    public async Task SeedAsync_running_twice_does_not_duplicate()
    {
        using var fixture = new SqliteDbContextFixture();
        var seeder = new ProductSeeder(fixture.DbContext);

        await seeder.SeedAsync();
        var firstCount = fixture.DbContext.Products.Count();

        await seeder.SeedAsync();
        var secondCount = fixture.DbContext.Products.Count();

        Assert.Equal(firstCount, secondCount);
    }
}
