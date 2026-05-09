using bleak.Api.Rest;
using EcommerceMaui.Models;

namespace EcommerceMaui.Services;

public class EcommerceApiClient : IEcommerceApiClient
{
    private const string DefaultBaseUrl = "https://sandbox.api.jamal.com";

    private readonly RestClient _client;
    private readonly string _baseUrl;

    public EcommerceApiClient(RestClient client, string? baseUrl = null)
    {
        _client = client;
        _baseUrl = (baseUrl ?? DefaultBaseUrl).TrimEnd('/');
    }

    public async Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        var result = await _client.ExecuteRestMethodAsync<List<Product>, ApiErrorResponse>(
            uri: new Uri($"{_baseUrl}/api/products"),
            verb: HttpVerbs.GET,
            accept: "application/json",
            cancellationToken: cancellationToken);

        EnsureSuccess(result);
        return result.Results ?? new List<Product>();
    }

    public async Task<Product?> GetProductAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = await _client.ExecuteRestMethodAsync<Product, ApiErrorResponse>(
            uri: new Uri($"{_baseUrl}/api/products/{id}"),
            verb: HttpVerbs.GET,
            accept: "application/json",
            cancellationToken: cancellationToken);

        if (result.Error is not null)
        {
            return null;
        }

        EnsureSuccess(result);
        return result.Results;
    }

    public async Task<SearchResponse> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var encoded = Uri.EscapeDataString(query ?? string.Empty);
        var result = await _client.ExecuteRestMethodAsync<SearchResponse, ApiErrorResponse>(
            uri: new Uri($"{_baseUrl}/api/search?q={encoded}"),
            verb: HttpVerbs.GET,
            accept: "application/json",
            cancellationToken: cancellationToken);

        EnsureSuccess(result);
        return result.Results ?? new SearchResponse { Query = query ?? string.Empty };
    }

    public async Task<LoginResponse> LoginAsync(string userName, CancellationToken cancellationToken = default)
    {
        var result = await _client.ExecuteRestMethodAsync<LoginResponse, ApiErrorResponse>(
            uri: new Uri($"{_baseUrl}/api/login"),
            verb: HttpVerbs.POST,
            payload: new LoginRequest { UserName = userName },
            accept: "application/json",
            cancellationToken: cancellationToken);

        EnsureSuccess(result);
        return result.Results ?? new LoginResponse { UserName = userName, Message = string.Empty };
    }

    private static void EnsureSuccess<TSuccess>(RestResults<TSuccess, ApiErrorResponse> result)
    {
        if (!string.IsNullOrWhiteSpace(result.UnhandledError))
        {
            throw new EcommerceApiException(result.UnhandledError!);
        }

        if (result.Error is not null)
        {
            throw new EcommerceApiException(result.Error.Message);
        }
    }
}

public class EcommerceApiException : Exception
{
    public EcommerceApiException(string message) : base(message) { }
}
