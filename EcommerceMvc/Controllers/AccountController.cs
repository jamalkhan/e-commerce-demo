using EcommerceMvc.Models;
using EcommerceMvc.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceMvc.Controllers;

public class AccountController : Controller
{
    private const string AuthCookieName = "auth_token";

    private readonly IAuthApiClient _api;

    public AccountController(IAuthApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _api.RegisterAsync(model.Email!, model.Name!, model.Password!, ct);
        if (!result.IsSuccess || result.Value is null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Could not create account.");
            return View(model);
        }

        SetAuthCookie(result.Value.Token, result.Value.ExpiresAt);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _api.LoginAsync(model.Email!, model.Password!, ct);
        if (!result.IsSuccess || result.Value is null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Invalid email or password.");
            return View(model);
        }

        SetAuthCookie(result.Value.Token, result.Value.ExpiresAt);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var token = Request.Cookies[AuthCookieName];
        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                await _api.LogoutAsync(token, ct);
            }
            catch
            {
                // best-effort: clear the cookie even if the API call fails
            }
        }
        Response.Cookies.Delete(AuthCookieName);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);

        await _api.RequestPasswordResetAsync(model.Email!, ct);
        return View("ForgotPasswordConfirmation");
    }

    [HttpGet]
    public IActionResult ResetPassword(string? token)
    {
        return View(new ResetPasswordViewModel { Token = token });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _api.ResetPasswordAsync(model.Token!, model.NewPassword!, ct);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Could not reset password.");
            return View(model);
        }

        return View("ResetPasswordConfirmation");
    }

    [HttpGet]
    public IActionResult OAuthCallback(string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            ModelState.AddModelError(string.Empty, "OAuth sign-in did not return a session token.");
            return View("Login", new LoginViewModel());
        }

        SetAuthCookie(token, DateTimeOffset.UtcNow.AddMinutes(30));
        return RedirectToAction("Index", "Home");
    }

    private void SetAuthCookie(string token, DateTimeOffset expiresAt)
    {
        Response.Cookies.Append(AuthCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = Request.IsHttps,
            Expires = expiresAt
        });
    }
}
