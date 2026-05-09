using EcommerceMvc.Controllers;
using EcommerceMvc.Data;
using EcommerceMvc.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace EcommerceMvc.Tests;

public class SearchControllerTests
{
    private static SearchController CreateController()
    {
        var controller = new SearchController(Substitute.For<ILogger<SearchController>>());
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
    public void Index_with_blank_query_returns_empty_results_with_message(string? query)
    {
        var controller = CreateController();

        var result = Assert.IsType<ViewResult>(controller.Index(query));
        var model = Assert.IsAssignableFrom<IEnumerable<Product>>(result.Model);

        Assert.Empty(model);
        Assert.Equal("Enter a product name to search.", controller.ViewData["Message"]);
    }

    [Fact]
    public void Index_returns_filtered_products_for_partial_match()
    {
        var controller = CreateController();
        var expectedMatches = ProductStore.Products
            .Where(p => p.Name.Contains("Smart", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var result = Assert.IsType<ViewResult>(controller.Index("Smart"));
        var model = Assert.IsAssignableFrom<IEnumerable<Product>>(result.Model);

        Assert.Equal(expectedMatches.Count, model.Count());
        Assert.Equal("Smart", controller.ViewData["Query"]);
    }

    [Fact]
    public void Index_search_is_case_insensitive()
    {
        var controller = CreateController();

        var lower = ((ViewResult)controller.Index("smart")).Model as IEnumerable<Product>;
        var upper = ((ViewResult)controller.Index("SMART")).Model as IEnumerable<Product>;

        Assert.NotNull(lower);
        Assert.NotNull(upper);
        Assert.Equal(lower!.Count(), upper!.Count());
    }

    [Fact]
    public void Index_returns_empty_result_when_no_match()
    {
        var controller = CreateController();

        var result = Assert.IsType<ViewResult>(controller.Index("ZzNoMatchZz"));
        var model = Assert.IsAssignableFrom<IEnumerable<Product>>(result.Model);

        Assert.Empty(model);
    }
}
