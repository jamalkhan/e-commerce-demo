using EcommerceMaui.Models;
using EcommerceMaui.Services;
using EcommerceMaui.ViewModels;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace EcommerceMaui.Core.Tests;

public class ProductsViewModelTests
{
    [Fact]
    public async Task LoadAsync_populates_products_from_api()
    {
        var api = Substitute.For<IEcommerceApiClient>();
        api.GetProductsAsync(Arg.Any<CancellationToken>()).Returns(new List<Product>
        {
            new() { Id = 1, Name = "Widget" },
            new() { Id = 2, Name = "Gadget" }
        });
        var vm = new ProductsViewModel(api, Substitute.For<INavigationService>());

        await vm.LoadAsync();

        Assert.Equal(2, vm.Products.Count);
        Assert.Equal("Widget", vm.Products[0].Name);
    }

    [Fact]
    public async Task LoadAsync_clears_existing_products_before_loading()
    {
        var api = Substitute.For<IEcommerceApiClient>();
        api.GetProductsAsync(Arg.Any<CancellationToken>()).Returns(
            new List<Product> { new() { Id = 1, Name = "First" } },
            new List<Product> { new() { Id = 99, Name = "Replacement" } });
        var vm = new ProductsViewModel(api, Substitute.For<INavigationService>());

        await vm.LoadAsync();
        await vm.LoadAsync();

        Assert.Single(vm.Products);
        Assert.Equal("Replacement", vm.Products[0].Name);
    }

    [Fact]
    public async Task LoadAsync_sets_ErrorMessage_on_exception()
    {
        var api = Substitute.For<IEcommerceApiClient>();
        api.GetProductsAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("server down"));
        var vm = new ProductsViewModel(api, Substitute.For<INavigationService>());

        await vm.LoadAsync();

        Assert.True(vm.HasError);
        Assert.Equal("server down", vm.ErrorMessage);
        Assert.Empty(vm.Products);
    }

    [Fact]
    public async Task LoadAsync_clears_IsBusy_when_done()
    {
        var api = Substitute.For<IEcommerceApiClient>();
        api.GetProductsAsync(Arg.Any<CancellationToken>()).Returns(new List<Product>());
        var vm = new ProductsViewModel(api, Substitute.For<INavigationService>());

        await vm.LoadAsync();

        Assert.False(vm.IsBusy);
    }

    [Fact]
    public async Task LoadAsync_does_nothing_when_already_busy()
    {
        var api = Substitute.For<IEcommerceApiClient>();
        api.GetProductsAsync(Arg.Any<CancellationToken>()).Returns(new List<Product>());
        var vm = new ProductsViewModel(api, Substitute.For<INavigationService>())
        {
            IsBusy = true
        };

        await vm.LoadAsync();

        await api.DidNotReceive().GetProductsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ViewDetailsCommand_navigates_with_product_id()
    {
        var nav = Substitute.For<INavigationService>();
        var vm = new ProductsViewModel(Substitute.For<IEcommerceApiClient>(), nav);

        vm.ViewDetailsCommand.Execute(new Product { Id = 42 });
        await Task.Delay(50);

        await nav.Received(1).GoToAsync("product?id=42");
    }

    [Fact]
    public async Task ViewDetailsCommand_does_nothing_when_product_null()
    {
        var nav = Substitute.For<INavigationService>();
        var vm = new ProductsViewModel(Substitute.For<IEcommerceApiClient>(), nav);

        vm.ViewDetailsCommand.Execute(null);
        await Task.Delay(50);

        await nav.DidNotReceive().GoToAsync(Arg.Any<string>());
    }
}
