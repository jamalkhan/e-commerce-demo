using System.Security.Claims;
using EcommerceApi.Authentication;
using EcommerceApi.Models;
using EcommerceApi.Services;

namespace EcommerceApi.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/auth");

        group.MapPost("/register", async (RegisterRequest request, IAuthenticationService auth, CancellationToken ct) =>
        {
            try
            {
                var result = await auth.RegisterAsync(request.Email ?? string.Empty, request.Name ?? string.Empty, request.Password ?? string.Empty, ct);
                return Results.Ok(BuildSession(result));
            }
            catch (AuthenticationException ex)
            {
                return Results.BadRequest(new ApiErrorResponse(ex.Message));
            }
        });

        group.MapPost("/login", async (LoginCredentialsRequest request, IAuthenticationService auth, CancellationToken ct) =>
        {
            try
            {
                var result = await auth.LoginAsync(request.Email ?? string.Empty, request.Password ?? string.Empty, ct);
                return Results.Ok(BuildSession(result));
            }
            catch (AuthenticationException)
            {
                return Results.Unauthorized();
            }
        });

        group.MapPost("/logout", async (HttpContext context, IAuthenticationService auth, CancellationToken ct) =>
        {
            var token = context.User.FindFirstValue("session_token");
            if (!string.IsNullOrEmpty(token))
            {
                await auth.LogoutAsync(token, ct);
            }
            return Results.NoContent();
        }).RequireAuthorization();

        group.MapGet("/me", (HttpContext context) =>
        {
            var idClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out var id))
            {
                return Results.Unauthorized();
            }

            var user = new UserDto(
                id,
                context.User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                context.User.FindFirstValue(ClaimTypes.Name) ?? string.Empty);
            return Results.Ok(user);
        }).RequireAuthorization();

        group.MapPost("/forgot-password", async (ForgotPasswordRequest request, IAuthenticationService auth, CancellationToken ct) =>
        {
            await auth.RequestPasswordResetAsync(request.Email ?? string.Empty, ct);
            return Results.NoContent();
        });

        group.MapPost("/reset-password", async (ResetPasswordRequest request, IAuthenticationService auth, CancellationToken ct) =>
        {
            try
            {
                var ok = await auth.CompletePasswordResetAsync(request.Token ?? string.Empty, request.NewPassword ?? string.Empty, ct);
                return ok
                    ? Results.Ok(new { ok = true })
                    : Results.BadRequest(new ApiErrorResponse("Invalid or expired reset token."));
            }
            catch (AuthenticationException ex)
            {
                return Results.BadRequest(new ApiErrorResponse(ex.Message));
            }
        });

        return routes;
    }

    private static SessionResponse BuildSession(AuthResult result) =>
        new(result.Session.Token,
            result.Session.ExpiresAt,
            new UserDto(result.User.Id, result.User.Email, result.User.Name));
}
