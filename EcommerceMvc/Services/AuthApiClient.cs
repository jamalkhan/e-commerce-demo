using bleak.Api.Rest;
using Microsoft.Extensions.Options;

namespace EcommerceMvc.Services;

public class AuthApiClient : IAuthApiClient
{
    private readonly RestClient _client;
    private readonly string _baseUrl;

    public AuthApiClient(RestClient client, IOptions<EcommerceApiOptions> options)
    {
        _client = client;
        _baseUrl = (options.Value.BaseUrl ?? "https://sandbox.api.jamal.com").TrimEnd('/');
    }

    public async Task<AuthApiResult<AuthApiSession>> RegisterAsync(string email, string name, string password, CancellationToken cancellationToken = default)
    {
        var result = await _client.ExecuteRestMethodAsync<AuthApiSession, AuthApiError>(
            uri: new Uri($"{_baseUrl}/api/auth/register"),
            verb: HttpVerbs.POST,
            payload: new { email, name, password },
            accept: "application/json",
            cancellationToken: cancellationToken);

        return ToResult(result);
    }

    public async Task<AuthApiResult<AuthApiSession>> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var result = await _client.ExecuteRestMethodAsync<AuthApiSession, AuthApiError>(
            uri: new Uri($"{_baseUrl}/api/auth/login"),
            verb: HttpVerbs.POST,
            payload: new { email, password },
            accept: "application/json",
            cancellationToken: cancellationToken);

        return ToResult(result, fallbackErrorMessage: "Invalid email or password.");
    }

    public async Task LogoutAsync(string sessionToken, CancellationToken cancellationToken = default)
    {
        await _client.ExecuteRestMethodAsync<object, AuthApiError>(
            uri: new Uri($"{_baseUrl}/api/auth/logout"),
            verb: HttpVerbs.POST,
            headers: new List<Header>
            {
                new() { Name = "Authorization", Value = $"Bearer {sessionToken}" }
            },
            accept: "application/json",
            cancellationToken: cancellationToken);
    }

    public async Task<AuthApiResult<AuthApiUser>> GetCurrentUserAsync(string sessionToken, CancellationToken cancellationToken = default)
    {
        var result = await _client.ExecuteRestMethodAsync<AuthApiUser, AuthApiError>(
            uri: new Uri($"{_baseUrl}/api/auth/me"),
            verb: HttpVerbs.GET,
            headers: new List<Header>
            {
                new() { Name = "Authorization", Value = $"Bearer {sessionToken}" }
            },
            accept: "application/json",
            cancellationToken: cancellationToken);

        return ToResult(result);
    }

    public async Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
    {
        await _client.ExecuteRestMethodAsync<object, AuthApiError>(
            uri: new Uri($"{_baseUrl}/api/auth/forgot-password"),
            verb: HttpVerbs.POST,
            payload: new { email },
            accept: "application/json",
            cancellationToken: cancellationToken);
    }

    public async Task<AuthApiResult<bool>> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default)
    {
        var result = await _client.ExecuteRestMethodAsync<object, AuthApiError>(
            uri: new Uri($"{_baseUrl}/api/auth/reset-password"),
            verb: HttpVerbs.POST,
            payload: new { token, newPassword },
            accept: "application/json",
            cancellationToken: cancellationToken);

        if (!string.IsNullOrEmpty(result.UnhandledError))
        {
            return new AuthApiResult<bool>(false, result.UnhandledError, 500);
        }

        if (result.Error is not null)
        {
            return new AuthApiResult<bool>(false, result.Error.Message, 400);
        }

        return new AuthApiResult<bool>(true, null, 200);
    }

    private static AuthApiResult<T> ToResult<T>(RestResults<T, AuthApiError> result, string? fallbackErrorMessage = null)
    {
        if (!string.IsNullOrEmpty(result.UnhandledError))
        {
            return new AuthApiResult<T>(default, result.UnhandledError, 500);
        }

        if (result.Error is not null)
        {
            return new AuthApiResult<T>(default, result.Error.Message, 400);
        }

        if (result.Results is null)
        {
            return new AuthApiResult<T>(default, fallbackErrorMessage ?? "The API returned no content.", 500);
        }

        return new AuthApiResult<T>(result.Results, null, 200);
    }
}

public class EcommerceApiOptions
{
    public string? BaseUrl { get; set; }
}
