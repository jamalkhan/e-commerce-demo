using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using IAuthenticationService = EcommerceApi.Services.IAuthenticationService;
using Microsoft.Extensions.Options;

namespace EcommerceApi.Authentication;

public class SessionAuthenticationOptions : AuthenticationSchemeOptions
{
    public string CookieName { get; set; } = "auth_token";
}

public class SessionAuthenticationHandler : AuthenticationHandler<SessionAuthenticationOptions>
{
    public const string SchemeName = "Session";

    private readonly IAuthenticationService _authService;

    public SessionAuthenticationHandler(
        IOptionsMonitor<SessionAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IAuthenticationService authService)
        : base(options, logger, encoder)
    {
        _authService = authService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = ExtractToken(Request, Options.CookieName);
        if (string.IsNullOrEmpty(token))
        {
            return AuthenticateResult.NoResult();
        }

        var session = await _authService.GetAndExtendSessionAsync(token, Context.RequestAborted);
        if (session is null || session.User is null)
        {
            return AuthenticateResult.Fail("Invalid or expired session.");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, session.UserId.ToString()),
            new Claim(ClaimTypes.Name, session.User.Name),
            new Claim(ClaimTypes.Email, session.User.Email),
            new Claim("session_token", session.Token)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return AuthenticateResult.Success(ticket);
    }

    private static string? ExtractToken(HttpRequest request, string cookieName)
    {
        if (request.Headers.TryGetValue("Authorization", out var auth))
        {
            var value = auth.ToString();
            const string prefix = "Bearer ";
            if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var token = value[prefix.Length..].Trim();
                if (!string.IsNullOrEmpty(token)) return token;
            }
        }

        if (request.Cookies.TryGetValue(cookieName, out var cookie) && !string.IsNullOrEmpty(cookie))
        {
            return cookie;
        }

        return null;
    }
}
