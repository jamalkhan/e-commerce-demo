using EcommerceMvc.Controllers;
using EcommerceMvc.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace EcommerceMvc.Tests;

public class HomeControllerTests
{
    private static HomeController CreateController()
    {
        var controller = new HomeController(Substitute.For<ILogger<HomeController>>());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    [Fact]
    public void Index_returns_ViewResult()
    {
        var controller = CreateController();

        var result = controller.Index();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Privacy_returns_ViewResult()
    {
        var controller = CreateController();

        var result = controller.Privacy();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Error_returns_ViewResult_with_ErrorViewModel_carrying_request_id()
    {
        var controller = CreateController();
        controller.HttpContext.TraceIdentifier = "trace-123";

        var result = Assert.IsType<ViewResult>(controller.Error());
        var model = Assert.IsType<ErrorViewModel>(result.Model);

        Assert.False(string.IsNullOrEmpty(model.RequestId));
    }
}
