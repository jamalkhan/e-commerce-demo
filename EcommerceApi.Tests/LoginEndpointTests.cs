using System.Net;
using System.Net.Http.Json;
using EcommerceApi.Models;
using EcommerceData.Repositories;
using NSubstitute;
using Xunit;

namespace EcommerceApi.Tests;

public class LoginEndpointTests
{
    [Fact]
    public async Task Login_post_returns_200_with_response_for_valid_username()
    {
        await using var factory = new EcommerceApiFactory(Substitute.For<IProductRepository>());
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/login", new { userName = "jamal" });
        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("jamal", payload!.UserName);
        Assert.False(string.IsNullOrEmpty(payload.Message));
    }

    [Fact]
    public async Task Login_post_returns_400_when_username_blank()
    {
        await using var factory = new EcommerceApiFactory(Substitute.For<IProductRepository>());
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/login", new { userName = "" });
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(error);
        Assert.False(string.IsNullOrEmpty(error!.Message));
    }

    [Fact]
    public async Task Login_post_trims_username_in_response()
    {
        await using var factory = new EcommerceApiFactory(Substitute.For<IProductRepository>());
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/login", new { userName = "  jamal  " });
        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(payload);
        Assert.Equal("jamal", payload!.UserName);
    }

    [Fact]
    public async Task Login_get_returns_200_with_response_for_valid_uname()
    {
        await using var factory = new EcommerceApiFactory(Substitute.For<IProductRepository>());
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/login?uname=jamal");
        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("jamal", payload!.UserName);
    }

    [Fact]
    public async Task Login_get_returns_400_when_uname_missing()
    {
        await using var factory = new EcommerceApiFactory(Substitute.For<IProductRepository>());
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/login");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
