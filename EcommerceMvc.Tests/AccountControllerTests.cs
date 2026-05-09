using EcommerceMvc.Controllers;
using EcommerceMvc.Models;
using EcommerceMvc.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace EcommerceMvc.Tests;

public class AccountControllerTests
{
    private static AccountController CreateController(IAuthApiClient api, HttpContext? httpContext = null)
    {
        return new AccountController(api)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext ?? new DefaultHttpContext() }
        };
    }

    private static AuthApiSession SuccessSession() =>
        new("session-tok", DateTimeOffset.UtcNow.AddMinutes(30),
            new AuthApiUser(1, "ada@example.com", "Ada"));

    [Fact]
    public void Register_GET_returns_view_with_empty_view_model()
    {
        var controller = CreateController(Substitute.For<IAuthApiClient>());

        var result = Assert.IsType<ViewResult>(controller.Register());
        Assert.IsType<RegisterViewModel>(result.Model);
    }

    [Fact]
    public async Task Register_POST_returns_view_when_model_invalid()
    {
        var api = Substitute.For<IAuthApiClient>();
        var controller = CreateController(api);
        controller.ModelState.AddModelError("Email", "required");

        var result = Assert.IsType<ViewResult>(await controller.Register(new RegisterViewModel(), CancellationToken.None));
        Assert.IsType<RegisterViewModel>(result.Model);
        await api.DidNotReceive().RegisterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Register_POST_redirects_to_home_and_sets_cookie_on_success()
    {
        var api = Substitute.For<IAuthApiClient>();
        api.RegisterAsync("ada@example.com", "Ada", "secret123", Arg.Any<CancellationToken>())
            .Returns(new AuthApiResult<AuthApiSession>(SuccessSession(), null, 200));
        var http = new DefaultHttpContext();
        var controller = CreateController(api, http);

        var result = await controller.Register(new RegisterViewModel
        {
            Email = "ada@example.com",
            Name = "Ada",
            Password = "secret123"
        }, CancellationToken.None);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Home", redirect.ControllerName);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Contains("auth_token=session-tok", http.Response.Headers.SetCookie.ToString());
    }

    [Fact]
    public async Task Register_POST_returns_view_with_error_when_api_fails()
    {
        var api = Substitute.For<IAuthApiClient>();
        api.RegisterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new AuthApiResult<AuthApiSession>(null, "Email already taken.", 400));
        var controller = CreateController(api);

        var result = Assert.IsType<ViewResult>(await controller.Register(new RegisterViewModel
        {
            Email = "ada@example.com",
            Name = "Ada",
            Password = "secret123"
        }, CancellationToken.None));

        Assert.False(controller.ModelState.IsValid);
        Assert.Contains(controller.ModelState[string.Empty]!.Errors,
            e => e.ErrorMessage.Contains("Email already taken"));
    }

    [Fact]
    public async Task Login_POST_redirects_to_home_and_sets_cookie_on_success()
    {
        var api = Substitute.For<IAuthApiClient>();
        api.LoginAsync("ada@example.com", "secret123", Arg.Any<CancellationToken>())
            .Returns(new AuthApiResult<AuthApiSession>(SuccessSession(), null, 200));
        var http = new DefaultHttpContext();
        var controller = CreateController(api, http);

        var result = await controller.Login(new LoginViewModel
        {
            Email = "ada@example.com",
            Password = "secret123"
        }, CancellationToken.None);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Contains("auth_token=session-tok", http.Response.Headers.SetCookie.ToString());
    }

    [Fact]
    public async Task Login_POST_returns_view_with_error_when_api_unauthorized()
    {
        var api = Substitute.For<IAuthApiClient>();
        api.LoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new AuthApiResult<AuthApiSession>(null, "Invalid email or password.", 401));
        var controller = CreateController(api);

        var result = Assert.IsType<ViewResult>(await controller.Login(new LoginViewModel
        {
            Email = "ada@example.com",
            Password = "wrong"
        }, CancellationToken.None));

        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Logout_calls_api_with_existing_token_and_clears_cookie()
    {
        var api = Substitute.For<IAuthApiClient>();
        var http = new DefaultHttpContext();
        http.Request.Headers["Cookie"] = "auth_token=existing-tok";
        var controller = CreateController(api, http);

        var result = await controller.Logout(CancellationToken.None);

        Assert.IsType<RedirectToActionResult>(result);
        await api.Received(1).LogoutAsync("existing-tok", Arg.Any<CancellationToken>());
        Assert.Contains("auth_token=", http.Response.Headers.SetCookie.ToString());
    }

    [Fact]
    public async Task Logout_does_not_call_api_when_no_cookie()
    {
        var api = Substitute.For<IAuthApiClient>();
        var controller = CreateController(api);

        await controller.Logout(CancellationToken.None);

        await api.DidNotReceive().LogoutAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ForgotPassword_POST_calls_api_and_returns_confirmation()
    {
        var api = Substitute.For<IAuthApiClient>();
        var controller = CreateController(api);

        var result = Assert.IsType<ViewResult>(await controller.ForgotPassword(
            new ForgotPasswordViewModel { Email = "ada@example.com" }, CancellationToken.None));

        Assert.Equal("ForgotPasswordConfirmation", result.ViewName);
        await api.Received(1).RequestPasswordResetAsync("ada@example.com", Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ResetPassword_GET_returns_view_with_token_in_view_model()
    {
        var controller = CreateController(Substitute.For<IAuthApiClient>());

        var result = Assert.IsType<ViewResult>(controller.ResetPassword("the-token"));
        var model = Assert.IsType<ResetPasswordViewModel>(result.Model);
        Assert.Equal("the-token", model.Token);
    }

    [Fact]
    public async Task ResetPassword_POST_returns_confirmation_on_success()
    {
        var api = Substitute.For<IAuthApiClient>();
        api.ResetPasswordAsync("tok", "newPassword1", Arg.Any<CancellationToken>())
            .Returns(new AuthApiResult<bool>(true, null, 200));
        var controller = CreateController(api);

        var result = Assert.IsType<ViewResult>(await controller.ResetPassword(new ResetPasswordViewModel
        {
            Token = "tok",
            NewPassword = "newPassword1"
        }, CancellationToken.None));

        Assert.Equal("ResetPasswordConfirmation", result.ViewName);
    }

    [Fact]
    public async Task ResetPassword_POST_returns_view_with_error_on_invalid_token()
    {
        var api = Substitute.For<IAuthApiClient>();
        api.ResetPasswordAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new AuthApiResult<bool>(false, "Invalid or expired reset token.", 400));
        var controller = CreateController(api);

        var result = Assert.IsType<ViewResult>(await controller.ResetPassword(new ResetPasswordViewModel
        {
            Token = "bad",
            NewPassword = "newPassword1"
        }, CancellationToken.None));

        Assert.False(controller.ModelState.IsValid);
        Assert.NotEqual("ResetPasswordConfirmation", result.ViewName);
    }

    [Fact]
    public void OAuthCallback_redirects_home_and_sets_cookie_when_token_present()
    {
        var http = new DefaultHttpContext();
        var controller = CreateController(Substitute.For<IAuthApiClient>(), http);

        var result = controller.OAuthCallback("oauth-token");

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Contains("auth_token=oauth-token", http.Response.Headers.SetCookie.ToString());
    }

    [Fact]
    public void OAuthCallback_returns_login_view_when_token_missing()
    {
        var controller = CreateController(Substitute.For<IAuthApiClient>());

        var result = Assert.IsType<ViewResult>(controller.OAuthCallback(null));
        Assert.Equal("Login", result.ViewName);
        Assert.False(controller.ModelState.IsValid);
    }
}
