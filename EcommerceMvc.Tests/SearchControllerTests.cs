using EcommerceData.Repositories;
using EcommerceMvc.Controllers;
using EcommerceMvc.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Entities = EcommerceData.Entities;
using Xunit;

namespace EcommerceMvc.Tests;

public class SearchControllerTests
{
    private static SearchController CreateController(IProductRepository repo)
    {
        var controller = new SearchController(Substitute.For<ILogger<SearchController>>(), repo);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Index_with_blank_query_returns_empty_results_with_message(string? query)
    {
        var repo = Substitute.For<IProductRepository>();
        var controller = CreateController(repo);

        var result = Assert.IsType<ViewResult>(await controller.Index(query));
        var model = Assert.IsAssignableFrom<IEnumerable<Product>>(result.Model);

        Assert.Empty(model);
        Assert.Equal("Enter a product name to search.", controller.ViewData["Message"]);
        await repo.DidNotReceive().SearchByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Index_with_query_calls_repository_and_returns_matches()
    {
        var repo = Substitute.For<IProductRepository>();
        repo.SearchByNameAsync("Smart", Arg.Any<CancellationToken>())
            .Returns(new List<Entities.Product>
            {
                new() { Id = 1, Name = "Smart Widget", Description = "d", Price = 1m }
            });
        var controller = CreateController(repo);

        var result = Assert.IsType<ViewResult>(await controller.Index("Smart"));
        var model = Assert.IsAssignableFrom<IEnumerable<Product>>(result.Model);

        Assert.Single(model);
        Assert.Equal("Smart", controller.ViewData["Query"]);
    }

    [Fact]
    public async Task Index_returns_empty_model_when_repository_returns_no_matches()
    {
        var repo = Substitute.For<IProductRepository>();
        repo.SearchByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new List<Entities.Product>());
        var controller = CreateController(repo);

        var result = Assert.IsType<ViewResult>(await controller.Index("Nothing"));
        var model = Assert.IsAssignableFrom<IEnumerable<Product>>(result.Model);

        Assert.Empty(model);
        Assert.Equal("Nothing", controller.ViewData["Query"]);
    }

    [Fact]
    public async Task Index_passes_query_through_to_repository_unchanged()
    {
        var repo = Substitute.For<IProductRepository>();
        repo.SearchByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new List<Entities.Product>());
        var controller = CreateController(repo);

        await controller.Index("smart");

        await repo.Received(1).SearchByNameAsync("smart", Arg.Any<CancellationToken>());
    }
}
