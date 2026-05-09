using EcommerceData.Entities;
using EcommerceData.Repositories;
using Xunit;

namespace EcommerceData.Tests;

public class EfProductRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_returns_all_products_ordered_by_id()
    {
        using var fixture = new SqliteDbContextFixture();
        fixture.DbContext.Products.AddRange(
            new Product { Name = "Beta", Description = "b", Price = 2.00m },
            new Product { Name = "Alpha", Description = "a", Price = 1.00m });
        await fixture.DbContext.SaveChangesAsync();

        var repo = new EfProductRepository(fixture.DbContext);

        var result = await repo.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Beta", result[0].Name);
        Assert.Equal("Alpha", result[1].Name);
    }

    [Fact]
    public async Task GetAllAsync_returns_empty_when_no_products()
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new EfProductRepository(fixture.DbContext);

        var result = await repo.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_returns_product_when_exists()
    {
        using var fixture = new SqliteDbContextFixture();
        var entity = new Product { Name = "Widget", Description = "desc", Price = 9.99m };
        fixture.DbContext.Products.Add(entity);
        await fixture.DbContext.SaveChangesAsync();

        var repo = new EfProductRepository(fixture.DbContext);

        var result = await repo.GetByIdAsync(entity.Id);

        Assert.NotNull(result);
        Assert.Equal("Widget", result.Name);
        Assert.Equal(9.99m, result.Price);
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_when_missing()
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new EfProductRepository(fixture.DbContext);

        var result = await repo.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task SearchByNameAsync_returns_partial_matches()
    {
        using var fixture = new SqliteDbContextFixture();
        fixture.DbContext.Products.AddRange(
            new Product { Name = "Smart Water Bottle", Description = "d", Price = 1m },
            new Product { Name = "Smart Speaker", Description = "d", Price = 1m },
            new Product { Name = "Notebook", Description = "d", Price = 1m });
        await fixture.DbContext.SaveChangesAsync();

        var repo = new EfProductRepository(fixture.DbContext);

        var result = await repo.SearchByNameAsync("Smart");

        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Contains("Smart", p.Name));
    }

    [Fact]
    public async Task SearchByNameAsync_returns_empty_when_no_matches()
    {
        using var fixture = new SqliteDbContextFixture();
        fixture.DbContext.Products.Add(new Product { Name = "Notebook", Description = "d", Price = 1m });
        await fixture.DbContext.SaveChangesAsync();

        var repo = new EfProductRepository(fixture.DbContext);

        var result = await repo.SearchByNameAsync("Pencil");

        Assert.Empty(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task SearchByNameAsync_returns_empty_when_query_is_blank(string? query)
    {
        using var fixture = new SqliteDbContextFixture();
        fixture.DbContext.Products.Add(new Product { Name = "Notebook", Description = "d", Price = 1m });
        await fixture.DbContext.SaveChangesAsync();

        var repo = new EfProductRepository(fixture.DbContext);

        var result = await repo.SearchByNameAsync(query!);

        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchByNameAsync_trims_whitespace_around_query()
    {
        using var fixture = new SqliteDbContextFixture();
        fixture.DbContext.Products.Add(new Product { Name = "Notebook", Description = "d", Price = 1m });
        await fixture.DbContext.SaveChangesAsync();

        var repo = new EfProductRepository(fixture.DbContext);

        var result = await repo.SearchByNameAsync("  Note  ");

        Assert.Single(result);
        Assert.Equal("Notebook", result[0].Name);
    }
}
