using EcommerceMaui.Services;
using EcommerceMaui.ViewModels;
using NSubstitute;
using Xunit;

namespace EcommerceMaui.Core.Tests;

public class NotFoundViewModelTests
{
    [Fact]
    public void Title_is_Not_Found()
    {
        var vm = new NotFoundViewModel(Substitute.For<INavigationService>());
        Assert.Equal("Not Found", vm.Title);
    }

    [Fact]
    public async Task HomeCommand_navigates_to_home()
    {
        var nav = Substitute.For<INavigationService>();
        var vm = new NotFoundViewModel(nav);

        vm.HomeCommand.Execute(null);
        await Task.Delay(50);

        await nav.Received(1).GoHomeAsync();
    }
}
