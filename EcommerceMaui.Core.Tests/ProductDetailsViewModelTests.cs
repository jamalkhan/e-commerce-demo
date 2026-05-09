using EcommerceMaui.Models;
using EcommerceMaui.Services;
using EcommerceMaui.ViewModels;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace EcommerceMaui.Core.Tests;

public class ProductDetailsViewModelTests
{
    [Fact]
    public async Task Setting_Id_loads_product_and_sets_title()
    {
        var api = Substitute.For<IEcommerceApiClient>();
        api.GetProductAsync(7, Arg.Any<CancellationToken>())
            .Returns(new Product { Id = 7, Name = "Widget", Description = "d", Price = 9.99m });

        var vm = new ProductDetailsViewModel(api, Substitute.For<INavigationService>())
        {
            Id = 7
        };

        await Task.Delay(100);

        Assert.NotNull(vm.Product);
        Assert.Equal("Widget", vm.Product!.Name);
        Assert.Equal("Widget", vm.Title);
        Assert.False(vm.NotFound);
    }

    [Fact]
    public async Task LoadAsync_sets_NotFound_when_api_returns_null()
    {
        var api = Substitute.For<IEcommerceApiClient>();
        api.GetProductAsync(99, Arg.Any<CancellationToken>()).Returns((Product?)null);

        var vm = new ProductDetailsViewModel(api, Substitute.For<INavigationService>())
        {
            Id = 99
        };

        await Task.Delay(100);

        Assert.True(vm.NotFound);
        Assert.Null(vm.Product);
    }

    [Fact]
    public async Task LoadAsync_sets_ErrorMessage_on_exception()
    {
        var api = Substitute.For<IEcommerceApiClient>();
        api.GetProductAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("network"));

        var vm = new ProductDetailsViewModel(api, Substitute.For<INavigationService>())
        {
            Id = 1
        };

        await Task.Delay(100);

        Assert.True(vm.HasError);
        Assert.Equal("network", vm.ErrorMessage);
    }

    [Fact]
    public async Task BackCommand_calls_navigation_GoBackAsync()
    {
        var nav = Substitute.For<INavigationService>();
        var vm = new ProductDetailsViewModel(Substitute.For<IEcommerceApiClient>(), nav);

        vm.BackCommand.Execute(null);
        await Task.Delay(50);

        await nav.Received(1).GoBackAsync();
    }
}
