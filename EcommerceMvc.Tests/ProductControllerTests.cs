using EcommerceData.Repositories;
using EcommerceMvc.Controllers;
using EcommerceMvc.Models;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Entities = EcommerceData.Entities;
using Xunit;

namespace EcommerceMvc.Tests;

public class ProductControllerTests
{
    [Fact]
    public async Task Index_returns_view_with_all_products_from_repository()
    {
        var repo = Substitute.For<IProductRepository>();
        repo.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Entities.Product>
        {
            new() { Id = 1, Name = "Widget", Description = "d", Price = 9.99m },
            new() { Id = 2, Name = "Gadget", Description = "d", Price = 14.99m }
        });
        var controller = new ProductController(repo);

        var result = Assert.IsType<ViewResult>(await controller.Index());
        var model = Assert.IsAssignableFrom<IEnumerable<Product>>(result.Model);

        Assert.Equal(2, model.Count());
        Assert.Equal("Widget", model.First().Name);
    }

    [Fact]
    public async Task Index_returns_empty_model_when_repository_empty()
    {
        var repo = Substitute.For<IProductRepository>();
        repo.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Entities.Product>());
        var controller = new ProductController(repo);

        var result = Assert.IsType<ViewResult>(await controller.Index());
        var model = Assert.IsAssignableFrom<IEnumerable<Product>>(result.Model);

        Assert.Empty(model);
    }

    [Fact]
    public async Task Details_returns_view_with_product_when_found()
    {
        var repo = Substitute.For<IProductRepository>();
        repo.GetByIdAsync(7, Arg.Any<CancellationToken>())
            .Returns(new Entities.Product { Id = 7, Name = "Found", Description = "d", Price = 1.23m });
        var controller = new ProductController(repo);

        var result = Assert.IsType<ViewResult>(await controller.Details(7));
        var model = Assert.IsType<Product>(result.Model);

        Assert.Equal(7, model.Id);
        Assert.Equal("Found", model.Name);
    }

    [Fact]
    public async Task Details_returns_NotFound_view_when_repository_returns_null()
    {
        var repo = Substitute.For<IProductRepository>();
        repo.GetByIdAsync(99999, Arg.Any<CancellationToken>()).Returns((Entities.Product?)null);
        var controller = new ProductController(repo);

        var result = Assert.IsType<ViewResult>(await controller.Details(99999));

        Assert.Equal("NotFound", result.ViewName);
    }
}
