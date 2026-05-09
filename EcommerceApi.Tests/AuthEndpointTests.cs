using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EcommerceApi.Models;
using EcommerceApi.Services;
using EcommerceData.Entities;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace EcommerceApi.Tests;

public class AuthEndpointTests
{
    [Fact]
    public async Task Register_returns_200_with_session_response_on_success()
    {
        var auth = Substitute.For<IAuthenticationService>();
        var user = new User { Id = 1, Email = "ada@example.com", Name = "Ada" };
        var session = new Session { Token = "tok", UserId = 1, ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30), User = user };
        auth.RegisterAsync("ada@example.com", "Ada", "secret123", Arg.Any<CancellationToken>())
            .Returns(new AuthResult(user, session));

        await using var factory = new EcommerceApiFactory { AuthenticationService = auth };
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { email = "ada@example.com", name = "Ada", password = "secret123" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<SessionResponse>();
        Assert.NotNull(payload);
        Assert.Equal("tok", payload!.Token);
        Assert.Equal("ada@example.com", payload.User.Email);
    }

    [Fact]
    public async Task Register_returns_400_on_AuthenticationException()
    {
        var auth = Substitute.For<IAuthenticationService>();
        auth.RegisterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new AuthenticationException("Email already in use"));

        await using var factory = new EcommerceApiFactory { AuthenticationService = auth };
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { email = "ada@example.com", name = "Ada", password = "secret123" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.Equal("Email already in use", error!.Message);
    }

    [Fact]
    public async Task Login_returns_200_with_session_on_success()
    {
        var auth = Substitute.For<IAuthenticationService>();
        var user = new User { Id = 1, Email = "ada@example.com", Name = "Ada" };
        var session = new Session { Token = "tok", UserId = 1, ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30), User = user };
        auth.LoginAsync("ada@example.com", "secret", Arg.Any<CancellationToken>())
            .Returns(new AuthResult(user, session));

        await using var factory = new EcommerceApiFactory { AuthenticationService = auth };
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email = "ada@example.com", password = "secret" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<SessionResponse>();
        Assert.Equal("tok", payload!.Token);
    }

    [Fact]
    public async Task Login_returns_401_on_AuthenticationException()
    {
        var auth = Substitute.For<IAuthenticationService>();
        auth.LoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new AuthenticationException("Bad creds"));

        await using var factory = new EcommerceApiFactory { AuthenticationService = auth };
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email = "ada@example.com", password = "wrong" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_returns_204_when_authenticated_and_calls_service()
    {
        var auth = Substitute.For<IAuthenticationService>();
        var user = new User { Id = 1, Email = "ada@example.com", Name = "Ada" };
        var session = new Session { Token = "tok", UserId = 1, ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30), User = user };
        auth.GetAndExtendSessionAsync("tok", Arg.Any<CancellationToken>()).Returns(session);

        await using var factory = new EcommerceApiFactory { AuthenticationService = auth };
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "tok");

        var response = await client.PostAsync("/api/auth/logout", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await auth.Received(1).LogoutAsync("tok", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Logout_returns_401_without_session_token()
    {
        var auth = Substitute.For<IAuthenticationService>();
        await using var factory = new EcommerceApiFactory { AuthenticationService = auth };
        var client = factory.CreateClient();

        var response = await client.PostAsync("/api/auth/logout", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_returns_user_when_authenticated()
    {
        var auth = Substitute.For<IAuthenticationService>();
        var user = new User { Id = 7, Email = "ada@example.com", Name = "Ada" };
        var session = new Session { Token = "tok", UserId = 7, ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30), User = user };
        auth.GetAndExtendSessionAsync("tok", Arg.Any<CancellationToken>()).Returns(session);

        await using var factory = new EcommerceApiFactory { AuthenticationService = auth };
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "tok");

        var response = await client.GetAsync("/api/auth/me");
        var payload = await response.Content.ReadFromJsonAsync<UserDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(7, payload!.Id);
        Assert.Equal("ada@example.com", payload.Email);
    }

    [Fact]
    public async Task Me_returns_401_without_token()
    {
        await using var factory = new EcommerceApiFactory { AuthenticationService = Substitute.For<IAuthenticationService>() };
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_returns_401_with_invalid_token()
    {
        var auth = Substitute.For<IAuthenticationService>();
        auth.GetAndExtendSessionAsync("bad-tok", Arg.Any<CancellationToken>()).Returns((Session?)null);
        await using var factory = new EcommerceApiFactory { AuthenticationService = auth };
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "bad-tok");

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Forgot_password_returns_204_and_calls_service()
    {
        var auth = Substitute.For<IAuthenticationService>();
        await using var factory = new EcommerceApiFactory { AuthenticationService = auth };
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/forgot-password", new { email = "ada@example.com" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await auth.Received(1).RequestPasswordResetAsync("ada@example.com", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reset_password_returns_200_when_service_succeeds()
    {
        var auth = Substitute.For<IAuthenticationService>();
        auth.CompletePasswordResetAsync("tok", "newpw1234", Arg.Any<CancellationToken>()).Returns(true);
        await using var factory = new EcommerceApiFactory { AuthenticationService = auth };
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/reset-password",
            new { token = "tok", newPassword = "newpw1234" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Reset_password_returns_400_when_token_invalid()
    {
        var auth = Substitute.For<IAuthenticationService>();
        auth.CompletePasswordResetAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        await using var factory = new EcommerceApiFactory { AuthenticationService = auth };
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/reset-password",
            new { token = "bad", newPassword = "newpw1234" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Reset_password_returns_400_on_AuthenticationException()
    {
        var auth = Substitute.For<IAuthenticationService>();
        auth.CompletePasswordResetAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new AuthenticationException("Password too short"));
        await using var factory = new EcommerceApiFactory { AuthenticationService = auth };
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/reset-password",
            new { token = "tok", newPassword = "x" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Session_token_is_accepted_via_auth_token_cookie()
    {
        var auth = Substitute.For<IAuthenticationService>();
        var user = new User { Id = 1, Email = "ada@example.com", Name = "Ada" };
        var session = new Session { Token = "cookie-tok", UserId = 1, ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30), User = user };
        auth.GetAndExtendSessionAsync("cookie-tok", Arg.Any<CancellationToken>()).Returns(session);

        await using var factory = new EcommerceApiFactory { AuthenticationService = auth };
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", "auth_token=cookie-tok");

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
