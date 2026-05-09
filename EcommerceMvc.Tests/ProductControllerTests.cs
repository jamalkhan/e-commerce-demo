using EcommerceMvc.Controllers;
using EcommerceMvc.Data;
using EcommerceMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace EcommerceMvc.Tests;

public class ProductControllerTests
{
    [Fact]
    public void Index_returns_ViewResult_with_all_products_as_model()
    {
        var controller = new ProductController();

        var result = Assert.IsType<ViewResult>(controller.Index());
        var model = Assert.IsAssignableFrom<IEnumerable<Product>>(result.Model);

        Assert.Equal(ProductStore.Products.Count, model.Count());
    }

    [Fact]
    public void Details_returns_view_with_product_when_id_exists()
    {
        var controller = new ProductController();
        var existing = ProductStore.Products.First();

        var result = Assert.IsType<ViewResult>(controller.Details(existing.Id));
        var model = Assert.IsType<Product>(result.Model);

        Assert.Equal(existing.Id, model.Id);
        Assert.Equal(existing.Name, model.Name);
    }

    [Fact]
    public void Details_returns_NotFound_view_when_id_missing()
    {
        var controller = new ProductController();

        var result = Assert.IsType<ViewResult>(controller.Details(99999));

        Assert.Equal("NotFound", result.ViewName);
    }
}
