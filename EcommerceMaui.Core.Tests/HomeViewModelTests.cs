using EcommerceMaui.Services;
using EcommerceMaui.ViewModels;
using NSubstitute;
using Xunit;

namespace EcommerceMaui.Core.Tests;

public class HomeViewModelTests
{
    [Fact]
    public void Title_is_Home()
    {
        var vm = new HomeViewModel(Substitute.For<INavigationService>());

        Assert.Equal("Home", vm.Title);
    }

    [Fact]
    public void SearchCommand_cannot_execute_when_query_blank()
    {
        var vm = new HomeViewModel(Substitute.For<INavigationService>());

        Assert.False(vm.SearchCommand.CanExecute(null));

        vm.SearchQuery = "   ";
        Assert.False(vm.SearchCommand.CanExecute(null));
    }

    [Fact]
    public void SearchCommand_can_execute_when_query_set()
    {
        var vm = new HomeViewModel(Substitute.For<INavigationService>());

        vm.SearchQuery = "headphones";

        Assert.True(vm.SearchCommand.CanExecute(null));
    }

    [Fact]
    public async Task SearchCommand_navigates_with_url_encoded_query()
    {
        var nav = Substitute.For<INavigationService>();
        var vm = new HomeViewModel(nav)
        {
            SearchQuery = "smart bottle"
        };

        vm.SearchCommand.Execute(null);
        await Task.Delay(50);

        await nav.Received(1).GoToAsync("search?q=smart%20bottle");
    }

    [Fact]
    public async Task SearchCommand_does_not_navigate_when_query_blank()
    {
        var nav = Substitute.For<INavigationService>();
        var vm = new HomeViewModel(nav);

        vm.SearchCommand.Execute(null);
        await Task.Delay(50);

        await nav.DidNotReceive().GoToAsync(Arg.Any<string>());
    }

    [Fact]
    public void LoginCommand_cannot_execute_when_username_blank()
    {
        var vm = new HomeViewModel(Substitute.For<INavigationService>());
        Assert.False(vm.LoginCommand.CanExecute(null));
    }

    [Fact]
    public async Task LoginCommand_navigates_with_url_encoded_username()
    {
        var nav = Substitute.For<INavigationService>();
        var vm = new HomeViewModel(nav) { UserName = "jamal khan" };

        vm.LoginCommand.Execute(null);
        await Task.Delay(50);

        await nav.Received(1).GoToAsync("login?uname=jamal%20khan");
    }

    [Fact]
    public void Setting_SearchQuery_raises_CanExecuteChanged()
    {
        var vm = new HomeViewModel(Substitute.For<INavigationService>());
        var fired = 0;
        vm.SearchCommand.CanExecuteChanged += (_, _) => fired++;

        vm.SearchQuery = "test";

        Assert.Equal(1, fired);
    }
}
