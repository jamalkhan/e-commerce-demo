using System.Net;
using System.Net.Http.Json;
using EcommerceApi.Models;
using EcommerceData.Entities;
using EcommerceData.Repositories;
using NSubstitute;
using Xunit;

namespace EcommerceApi.Tests;

public class ProductsEndpointTests
{
    [Fact]
    public async Task GetProducts_returns_200_with_all_products_from_repository()
    {
        var repo = Substitute.For<IProductRepository>();
        repo.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Product>
        {
            new() { Id = 1, Name = "Widget", Description = "d", Price = 9.99m },
            new() { Id = 2, Name = "Gadget", Description = "d", Price = 14.99m }
        });

        await using var factory = new EcommerceApiFactory(repo);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/products");
        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(products);
        Assert.Equal(2, products!.Count);
        Assert.Equal("Widget", products[0].Name);
        Assert.Equal(9.99m, products[0].Price);
    }

    [Fact]
    public async Task GetProducts_returns_empty_array_when_repository_has_none()
    {
        var repo = Substitute.For<IProductRepository>();
        repo.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Product>());

        await using var factory = new EcommerceApiFactory(repo);
        var client = factory.CreateClient();

        var products = await client.GetFromJsonAsync<List<ProductDto>>("/api/products");

        Assert.NotNull(products);
        Assert.Empty(products!);
    }

    [Fact]
    public async Task GetProductById_returns_200_with_product_when_found()
    {
        var repo = Substitute.For<IProductRepository>();
        repo.GetByIdAsync(7, Arg.Any<CancellationToken>())
            .Returns(new Product { Id = 7, Name = "Found", Description = "yes", Price = 1.23m });

        await using var factory = new EcommerceApiFactory(repo);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/products/7");
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(product);
        Assert.Equal(7, product!.Id);
        Assert.Equal("Found", product.Name);
    }

    [Fact]
    public async Task GetProductById_returns_404_with_error_message_when_missing()
    {
        var repo = Substitute.For<IProductRepository>();
        repo.GetByIdAsync(999, Arg.Any<CancellationToken>()).Returns((Product?)null);

        await using var factory = new EcommerceApiFactory(repo);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/products/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error);
        Assert.Contains("999", error!.Message);
    }
}
