using EcommerceMvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace EcommerceMvc.Tests;

public class LoginControllerTests
{
    [Fact]
    public void Index_returns_ViewResult_for_any_uname()
    {
        var controller = new LoginController(Substitute.For<ILogger<SearchController>>());

        var result = controller.Index("jamal");

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Index_returns_ViewResult_for_empty_uname()
    {
        var controller = new LoginController(Substitute.For<ILogger<SearchController>>());

        var result = controller.Index(string.Empty);

        Assert.IsType<ViewResult>(result);
    }
}
