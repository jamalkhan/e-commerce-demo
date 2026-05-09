using EcommerceMaui.Models;
using EcommerceMaui.Services;
using EcommerceMaui.ViewModels;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace EcommerceMaui.Core.Tests;

public class LoginViewModelTests
{
    [Fact]
    public async Task LoginAsync_sets_welcome_message_from_api()
    {
        var api = Substitute.For<IEcommerceApiClient>();
        api.LoginAsync("jamal", Arg.Any<CancellationToken>())
            .Returns(new LoginResponse { UserName = "jamal", Message = "Welcome back" });

        var vm = new LoginViewModel(api, Substitute.For<INavigationService>())
        {
            UserName = "jamal"
        };

        await Task.Delay(100);

        Assert.Equal("Welcome back", vm.WelcomeMessage);
        Assert.Equal("jamal", vm.UserName);
    }

    [Fact]
    public async Task LoginAsync_sets_ErrorMessage_on_exception()
    {
        var api = Substitute.For<IEcommerceApiClient>();
        api.LoginAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("auth failed"));

        var vm = new LoginViewModel(api, Substitute.For<INavigationService>())
        {
            UserName = "jamal"
        };

        await Task.Delay(100);

        Assert.True(vm.HasError);
        Assert.Equal("auth failed", vm.ErrorMessage);
        Assert.Null(vm.WelcomeMessage);
    }

    [Fact]
    public async Task LoginAsync_does_nothing_when_username_blank()
    {
        var api = Substitute.For<IEcommerceApiClient>();
        var vm = new LoginViewModel(api, Substitute.For<INavigationService>());

        await vm.LoginAsync();

        await api.DidNotReceive().LoginAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BackCommand_calls_navigation_GoHomeAsync()
    {
        var nav = Substitute.For<INavigationService>();
        var vm = new LoginViewModel(Substitute.For<IEcommerceApiClient>(), nav);

        vm.BackCommand.Execute(null);
        await Task.Delay(50);

        await nav.Received(1).GoHomeAsync();
    }
}
