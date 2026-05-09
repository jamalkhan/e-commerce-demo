using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using IAuthenticationService = EcommerceApi.Services.IAuthenticationService;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;

namespace EcommerceApi.Endpoints;

public static class OAuthEndpoints
{
    public const string ExternalCookieScheme = "ExternalCookie";

    public static IEndpointRouteBuilder MapOAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/auth/oauth");

        group.MapGet("/{provider}", (string provider, string? returnUrl, HttpContext http) =>
        {
            var scheme = provider.Equals("google", StringComparison.OrdinalIgnoreCase)
                ? GoogleDefaults.AuthenticationScheme
                : provider.Equals("facebook", StringComparison.OrdinalIgnoreCase)
                    ? FacebookDefaults.AuthenticationScheme
                    : null;

            if (scheme is null)
            {
                return Results.BadRequest(new { message = $"Unknown provider '{provider}'." });
            }

            var redirect = $"/api/auth/oauth/{provider}/callback?returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}";
            return Results.Challenge(
                new AuthenticationProperties { RedirectUri = redirect },
                new[] { scheme });
        });

        group.MapGet("/{provider}/callback", async (string provider, string? returnUrl, HttpContext http, IAuthenticationService authService, CancellationToken ct) =>
        {
            var auth = await http.AuthenticateAsync(ExternalCookieScheme);
            if (!auth.Succeeded || auth.Principal is null)
            {
                return Results.Redirect(returnUrl ?? "/");
            }

            var providerNormalized = provider.Equals("google", StringComparison.OrdinalIgnoreCase) ? "Google"
                : provider.Equals("facebook", StringComparison.OrdinalIgnoreCase) ? "Facebook"
                : provider;

            var providerUserId = auth.Principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var email = auth.Principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            var name = auth.Principal.FindFirstValue(ClaimTypes.Name) ?? auth.Principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty;

            await http.SignOutAsync(ExternalCookieScheme);

            var result = await authService.SignInExternalAsync(providerNormalized, providerUserId, email, name, ct);

            var separator = (returnUrl ?? "/").Contains('?') ? "&" : "?";
            var target = $"{returnUrl ?? "/"}{separator}token={Uri.EscapeDataString(result.Session.Token)}";
            return Results.Redirect(target);
        });

        return routes;
    }
}

public class OAuthOptions
{
    public ProviderOptions? Google { get; set; }
    public ProviderOptions? Facebook { get; set; }
}

public class ProviderOptions
{
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
}
