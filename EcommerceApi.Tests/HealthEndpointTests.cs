using System.Net;
using System.Net.Http.Json;
using EcommerceData.Repositories;
using NSubstitute;
using Xunit;

namespace EcommerceApi.Tests;

public class HealthEndpointTests
{
    [Fact]
    public async Task Health_returns_ok_status()
    {
        await using var factory = new EcommerceApiFactory(Substitute.For<IProductRepository>());
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.NotNull(body);
        Assert.Equal("ok", body!.Status);
    }

    private record HealthResponse(string Status);
}
