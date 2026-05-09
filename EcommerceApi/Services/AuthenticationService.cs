using EcommerceData.Entities;
using EcommerceData.Repositories;
using EcommerceMail;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace EcommerceApi.Services;

public interface IAuthenticationService
{
    Task<AuthResult> RegisterAsync(string email, string name, string password, CancellationToken cancellationToken = default);
    Task<AuthResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task LogoutAsync(string sessionToken, CancellationToken cancellationToken = default);
    Task<Session?> GetAndExtendSessionAsync(string sessionToken, CancellationToken cancellationToken = default);
    Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> CompletePasswordResetAsync(string token, string newPassword, CancellationToken cancellationToken = default);
    Task<AuthResult> SignInExternalAsync(string provider, string providerUserId, string email, string name, CancellationToken cancellationToken = default);
}

public class AuthResult
{
    public AuthResult(User user, Session session)
    {
        User = user;
        Session = session;
    }

    public User User { get; }
    public Session Session { get; }
}

public class AuthenticationException : Exception
{
    public AuthenticationException(string message) : base(message) { }
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _users;
    private readonly ISessionRepository _sessions;
    private readonly IPasswordResetTokenRepository _resetTokens;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ISecureTokenGenerator _tokens;
    private readonly IEmailSender _email;
    private readonly AuthOptions _options;
    private readonly TimeProvider _time;

    public AuthenticationService(
        IUserRepository users,
        ISessionRepository sessions,
        IPasswordResetTokenRepository resetTokens,
        IPasswordHasher<User> passwordHasher,
        ISecureTokenGenerator tokens,
        IEmailSender email,
        IOptions<AuthOptions> options,
        TimeProvider time)
    {
        _users = users;
        _sessions = sessions;
        _resetTokens = resetTokens;
        _passwordHasher = passwordHasher;
        _tokens = tokens;
        _email = email;
        _options = options.Value;
        _time = time;
    }

    public async Task<AuthResult> RegisterAsync(string email, string name, string password, CancellationToken cancellationToken = default)
    {
        ValidateRegistrationInput(email, name, password);

        var existing = await _users.GetByEmailAsync(email, cancellationToken);
        if (existing is not null)
        {
            throw new AuthenticationException("An account with that email already exists.");
        }

        var user = new User
        {
            Email = email.Trim().ToLowerInvariant(),
            Name = name.Trim(),
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        await _users.AddAsync(user, cancellationToken);

        await _email.SendAsync(new EmailMessage
        {
            To = user.Email,
            ToName = user.Name,
            Subject = "Welcome to Ecommerce Demo",
            BodyText = $"Hi {user.Name},\n\nThanks for creating an account. You can sign in any time at https://sandbox.mvc.jamal.com.\n",
            BodyHtml = $"<p>Hi {System.Net.WebUtility.HtmlEncode(user.Name)},</p><p>Thanks for creating an account. You can sign in any time at <a href=\"https://sandbox.mvc.jamal.com\">sandbox.mvc.jamal.com</a>.</p>"
        }, cancellationToken);

        var session = await CreateSessionAsync(user, cancellationToken);
        return new AuthResult(user, session);
    }

    public async Task<AuthResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            throw new AuthenticationException("Email and password are required.");
        }

        var user = await _users.GetByEmailAsync(email, cancellationToken);
        if (user is null || string.IsNullOrEmpty(user.PasswordHash))
        {
            throw new AuthenticationException("Invalid email or password.");
        }

        var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verify == PasswordVerificationResult.Failed)
        {
            throw new AuthenticationException("Invalid email or password.");
        }

        if (verify == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            await _users.UpdateAsync(user, cancellationToken);
        }

        var session = await CreateSessionAsync(user, cancellationToken);
        return new AuthResult(user, session);
    }

    public Task LogoutAsync(string sessionToken, CancellationToken cancellationToken = default) =>
        _sessions.DeleteAsync(sessionToken, cancellationToken);

    public async Task<Session?> GetAndExtendSessionAsync(string sessionToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sessionToken)) return null;

        var session = await _sessions.GetByTokenAsync(sessionToken, cancellationToken);
        if (session is null) return null;

        var now = _time.GetUtcNow();
        if (session.ExpiresAt <= now)
        {
            await _sessions.DeleteAsync(session.Token, cancellationToken);
            return null;
        }

        session.LastActivityAt = now;
        session.ExpiresAt = now + _options.SessionDuration;
        await _sessions.UpdateActivityAsync(session, cancellationToken);
        return session;
    }

    public async Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        var user = await _users.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            // Do not reveal whether the email exists.
            return;
        }

        await _resetTokens.DeleteForUserAsync(user.Id, cancellationToken);

        var now = _time.GetUtcNow();
        var token = _tokens.Generate();
        await _resetTokens.AddAsync(new PasswordResetToken
        {
            Token = token,
            UserId = user.Id,
            CreatedAt = now,
            ExpiresAt = now + _options.PasswordResetTokenDuration
        }, cancellationToken);

        var resetUrl = _options.PasswordResetUrlTemplate.Replace("{token}", Uri.EscapeDataString(token));

        await _email.SendAsync(new EmailMessage
        {
            To = user.Email,
            ToName = user.Name,
            Subject = "Reset your password",
            BodyText = $"Hi {user.Name},\n\nUse this link to reset your password: {resetUrl}\nThe link expires in {_options.PasswordResetTokenDuration.TotalMinutes:F0} minutes.\n",
            BodyHtml = $"<p>Hi {System.Net.WebUtility.HtmlEncode(user.Name)},</p><p>Use this link to reset your password: <a href=\"{resetUrl}\">Reset password</a></p><p>The link expires in {_options.PasswordResetTokenDuration.TotalMinutes:F0} minutes.</p>"
        }, cancellationToken);
    }

    public async Task<bool> CompletePasswordResetAsync(string token, string newPassword, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(newPassword))
        {
            return false;
        }

        var resetToken = await _resetTokens.GetByTokenAsync(token, cancellationToken);
        if (resetToken is null) return false;
        if (resetToken.UsedAt is not null) return false;
        if (resetToken.ExpiresAt <= _time.GetUtcNow()) return false;

        var user = resetToken.User;
        if (user is null) return false;

        ValidatePassword(newPassword);

        user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
        await _users.UpdateAsync(user, cancellationToken);

        resetToken.UsedAt = _time.GetUtcNow();
        await _resetTokens.MarkUsedAsync(resetToken, cancellationToken);
        return true;
    }

    public async Task<AuthResult> SignInExternalAsync(string provider, string providerUserId, string email, string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(providerUserId))
        {
            throw new AuthenticationException("Provider and provider user id are required.");
        }

        var user = await _users.GetByExternalLoginAsync(provider, providerUserId, cancellationToken);
        if (user is null)
        {
            user = !string.IsNullOrWhiteSpace(email)
                ? await _users.GetByEmailAsync(email, cancellationToken)
                : null;

            if (user is null)
            {
                user = new User
                {
                    Email = (email ?? $"{providerUserId}@{provider}.local").Trim().ToLowerInvariant(),
                    Name = string.IsNullOrWhiteSpace(name) ? provider : name.Trim()
                };
                await _users.AddAsync(user, cancellationToken);
            }

            await _users.AddExternalLoginAsync(new UserExternalLogin
            {
                UserId = user.Id,
                Provider = provider,
                ProviderUserId = providerUserId
            }, cancellationToken);
        }

        var session = await CreateSessionAsync(user, cancellationToken);
        return new AuthResult(user, session);
    }

    private async Task<Session> CreateSessionAsync(User user, CancellationToken cancellationToken)
    {
        var now = _time.GetUtcNow();
        var session = new Session
        {
            Token = _tokens.Generate(),
            UserId = user.Id,
            CreatedAt = now,
            LastActivityAt = now,
            ExpiresAt = now + _options.SessionDuration
        };
        await _sessions.AddAsync(session, cancellationToken);
        return session;
    }

    private static void ValidateRegistrationInput(string email, string name, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            throw new AuthenticationException("A valid email address is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new AuthenticationException("Name is required.");
        }

        ValidatePassword(password);
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8)
        {
            throw new AuthenticationException("Password must be at least 8 characters long.");
        }
    }
}
