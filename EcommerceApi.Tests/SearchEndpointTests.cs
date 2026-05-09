using System.Net;
using System.Net.Http.Json;
using EcommerceApi.Models;
using EcommerceData.Entities;
using EcommerceData.Repositories;
using NSubstitute;
using Xunit;

namespace EcommerceApi.Tests;

public class SearchEndpointTests
{
    [Fact]
    public async Task Search_returns_default_message_when_query_blank()
    {
        var repo = Substitute.For<IProductRepository>();
        await using var factory = new EcommerceApiFactory(repo);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/search?q=");
        var payload = await response.Content.ReadFromJsonAsync<SearchResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(string.Empty, payload!.Query);
        Assert.Equal("Enter a product name to search.", payload.Message);
        Assert.Empty(payload.Results);

        await repo.DidNotReceive().SearchByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Search_returns_results_when_repository_has_matches()
    {
        var repo = Substitute.For<IProductRepository>();
        repo.SearchByNameAsync("Smart", Arg.Any<CancellationToken>())
            .Returns(new List<Product>
            {
                new() { Id = 1, Name = "Smart Widget", Description = "d", Price = 1m }
            });

        await using var factory = new EcommerceApiFactory(repo);
        var client = factory.CreateClient();

        var payload = await client.GetFromJsonAsync<SearchResponse>("/api/search?q=Smart");

        Assert.NotNull(payload);
        Assert.Equal("Smart", payload!.Query);
        Assert.Null(payload.Message);
        Assert.Single(payload.Results);
        Assert.Equal("Smart Widget", payload.Results[0].Name);
    }

    [Fact]
    public async Task Search_returns_not_found_message_when_no_matches()
    {
        var repo = Substitute.For<IProductRepository>();
        repo.SearchByNameAsync("Nothing", Arg.Any<CancellationToken>())
            .Returns(new List<Product>());

        await using var factory = new EcommerceApiFactory(repo);
        var client = factory.CreateClient();

        var payload = await client.GetFromJsonAsync<SearchResponse>("/api/search?q=Nothing");

        Assert.NotNull(payload);
        Assert.Equal("Nothing", payload!.Query);
        Assert.NotNull(payload.Message);
        Assert.Contains("Nothing", payload.Message!);
        Assert.Empty(payload.Results);
    }

    [Fact]
    public async Task Search_trims_whitespace_from_query()
    {
        var repo = Substitute.For<IProductRepository>();
        repo.SearchByNameAsync("Smart", Arg.Any<CancellationToken>())
            .Returns(new List<Product>());

        await using var factory = new EcommerceApiFactory(repo);
        var client = factory.CreateClient();

        var payload = await client.GetFromJsonAsync<SearchResponse>("/api/search?q=%20%20Smart%20%20");

        Assert.NotNull(payload);
        Assert.Equal("Smart", payload!.Query);
        await repo.Received(1).SearchByNameAsync("Smart", Arg.Any<CancellationToken>());
    }
}
