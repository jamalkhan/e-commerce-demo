using EcommerceMaui.Models;
using EcommerceMaui.Services;
using EcommerceMaui.ViewModels;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace EcommerceMaui.Core.Tests;

public class SearchViewModelTests
{
    [Fact]
    public async Task Setting_Query_triggers_search_and_populates_results()
    {
        var api = Substitute.For<IEcommerceApiClient>();
        api.SearchAsync("Smart", Arg.Any<CancellationToken>()).Returns(new SearchResponse
        {
            Query = "Smart",
            Message = null,
            Results = new List<Product> { new() { Id = 1, Name = "Smart Widget" } }
        });

        var vm = new SearchViewModel(api, Substitute.For<INavigationService>())
        {
            Query = "Smart"
        };

        await Task.Delay(100);

        Assert.Single(vm.Results);
        Assert.Null(vm.ResultsMessage);
    }

    [Fact]
    public async Task LoadAsync_propagates_api_message_when_no_matches()
    {
        var api = Substitute.For<IEcommerceApiClient>();
        api.SearchAsync("Nothing", Arg.Any<CancellationToken>()).Returns(new SearchResponse
        {
            Query = "Nothing",
            Message = "No products found.",
            Results = new List<Product>()
        });

        var vm = new SearchViewModel(api, Substitute.For<INavigationService>())
        {
            Query = "Nothing"
        };

        await Task.Delay(100);

        Assert.Empty(vm.Results);
        Assert.Equal("No products found.", vm.ResultsMessage);
    }

    [Fact]
    public async Task LoadAsync_sets_ErrorMessage_on_exception()
    {
        var api = Substitute.For<IEcommerceApiClient>();
        api.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("oops"));

        var vm = new SearchViewModel(api, Substitute.For<INavigationService>())
        {
            Query = "x"
        };

        await Task.Delay(100);

        Assert.True(vm.HasError);
        Assert.Equal("oops", vm.ErrorMessage);
    }

    [Fact]
    public async Task ViewDetailsCommand_navigates_with_product_id()
    {
        var nav = Substitute.For<INavigationService>();
        var vm = new SearchViewModel(Substitute.For<IEcommerceApiClient>(), nav);

        vm.ViewDetailsCommand.Execute(new Product { Id = 5 });
        await Task.Delay(50);

        await nav.Received(1).GoToAsync("product?id=5");
    }

    [Fact]
    public async Task BackCommand_calls_navigation_GoBackAsync()
    {
        var nav = Substitute.For<INavigationService>();
        var vm = new SearchViewModel(Substitute.For<IEcommerceApiClient>(), nav);

        vm.BackCommand.Execute(null);
        await Task.Delay(50);

        await nav.Received(1).GoBackAsync();
    }
}
