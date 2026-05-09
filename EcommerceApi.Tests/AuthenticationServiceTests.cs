using EcommerceApi.Services;
using EcommerceData.Entities;
using EcommerceData.Repositories;
using EcommerceMail;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace EcommerceApi.Tests;

public class AuthenticationServiceTests
{
    private static (AuthenticationService Service,
        IUserRepository Users,
        ISessionRepository Sessions,
        IPasswordResetTokenRepository ResetTokens,
        IPasswordHasher<User> Hasher,
        ISecureTokenGenerator Tokens,
        IEmailSender Email,
        FakeTimeProvider Time)
        BuildService(AuthOptions? authOptions = null)
    {
        var users = Substitute.For<IUserRepository>();
        var sessions = Substitute.For<ISessionRepository>();
        var resetTokens = Substitute.For<IPasswordResetTokenRepository>();
        var hasher = Substitute.For<IPasswordHasher<User>>();
        var tokens = Substitute.For<ISecureTokenGenerator>();
        var email = Substitute.For<IEmailSender>();
        var time = new FakeTimeProvider(new DateTimeOffset(2026, 5, 9, 12, 0, 0, TimeSpan.Zero));
        var options = Options.Create(authOptions ?? new AuthOptions());

        users.AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var u = call.Arg<User>();
                if (u.Id == 0) u.Id = Random.Shared.Next(1, int.MaxValue);
                return u;
            });
        users.AddExternalLoginAsync(Arg.Any<UserExternalLogin>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<UserExternalLogin>());

        sessions.AddAsync(Arg.Any<Session>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Session>());
        resetTokens.AddAsync(Arg.Any<PasswordResetToken>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<PasswordResetToken>());

        var service = new AuthenticationService(users, sessions, resetTokens, hasher, tokens, email, options, time);
        return (service, users, sessions, resetTokens, hasher, tokens, email, time);
    }

    [Fact]
    public async Task RegisterAsync_creates_user_with_hashed_password_and_sends_welcome_email()
    {
        var b = BuildService();
        b.Hasher.HashPassword(Arg.Any<User>(), "secret123").Returns("hashed-secret");
        b.Tokens.Generate(Arg.Any<int>()).Returns("session-tok");

        var result = await b.Service.RegisterAsync("Ada@Example.com", " Ada Lovelace ", "secret123");

        Assert.Equal("ada@example.com", result.User.Email);
        Assert.Equal("Ada Lovelace", result.User.Name);
        Assert.Equal("hashed-secret", result.User.PasswordHash);
        Assert.Equal("session-tok", result.Session.Token);
        await b.Email.Received(1).SendAsync(Arg.Is<EmailMessage>(m => m.To == "ada@example.com" && m.Subject.Contains("Welcome")), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_throws_when_email_already_in_use()
    {
        var b = BuildService();
        b.Users.GetByEmailAsync("ada@example.com", Arg.Any<CancellationToken>())
            .Returns(new User { Id = 1, Email = "ada@example.com", Name = "Ada" });

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            b.Service.RegisterAsync("ada@example.com", "Ada", "secret123"));
    }

    [Theory]
    [InlineData("", "Ada", "secret123")]
    [InlineData("not-an-email", "Ada", "secret123")]
    [InlineData("a@b.c", "", "secret123")]
    [InlineData("a@b.c", "Ada", "short")]
    public async Task RegisterAsync_rejects_invalid_input(string email, string name, string password)
    {
        var b = BuildService();
        await Assert.ThrowsAsync<AuthenticationException>(() =>
            b.Service.RegisterAsync(email, name, password));
    }

    [Fact]
    public async Task LoginAsync_returns_session_when_password_verifies()
    {
        var b = BuildService();
        var user = new User { Id = 5, Email = "ada@example.com", Name = "Ada", PasswordHash = "hashed" };
        b.Users.GetByEmailAsync("ada@example.com", Arg.Any<CancellationToken>()).Returns(user);
        b.Hasher.VerifyHashedPassword(user, "hashed", "secret123").Returns(PasswordVerificationResult.Success);
        b.Tokens.Generate(Arg.Any<int>()).Returns("login-tok");

        var result = await b.Service.LoginAsync("ada@example.com", "secret123");

        Assert.Equal(5, result.User.Id);
        Assert.Equal("login-tok", result.Session.Token);
    }

    [Fact]
    public async Task LoginAsync_rehashes_password_when_hasher_indicates_needed()
    {
        var b = BuildService();
        var user = new User { Id = 5, Email = "ada@example.com", Name = "Ada", PasswordHash = "old-hash" };
        b.Users.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        b.Hasher.VerifyHashedPassword(user, "old-hash", "secret123").Returns(PasswordVerificationResult.SuccessRehashNeeded);
        b.Hasher.HashPassword(user, "secret123").Returns("new-hash");
        b.Tokens.Generate(Arg.Any<int>()).Returns("tok");

        await b.Service.LoginAsync(user.Email, "secret123");

        await b.Users.Received(1).UpdateAsync(Arg.Is<User>(u => u.PasswordHash == "new-hash"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginAsync_throws_when_user_not_found()
    {
        var b = BuildService();
        b.Users.GetByEmailAsync("ghost@example.com", Arg.Any<CancellationToken>()).Returns((User?)null);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            b.Service.LoginAsync("ghost@example.com", "anything"));
    }

    [Fact]
    public async Task LoginAsync_throws_when_password_does_not_verify()
    {
        var b = BuildService();
        var user = new User { Id = 5, Email = "ada@example.com", PasswordHash = "h" };
        b.Users.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        b.Hasher.VerifyHashedPassword(user, "h", "wrong").Returns(PasswordVerificationResult.Failed);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            b.Service.LoginAsync(user.Email, "wrong"));
    }

    [Fact]
    public async Task LoginAsync_throws_when_user_has_no_password_hash()
    {
        var b = BuildService();
        var user = new User { Id = 5, Email = "ada@example.com", PasswordHash = null };
        b.Users.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            b.Service.LoginAsync(user.Email, "anything"));
    }

    [Fact]
    public async Task LogoutAsync_calls_session_repository_delete()
    {
        var b = BuildService();

        await b.Service.LogoutAsync("token-xyz");

        await b.Sessions.Received(1).DeleteAsync("token-xyz", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAndExtendSessionAsync_extends_expiration_on_active_session()
    {
        var b = BuildService(new AuthOptions { SessionDuration = TimeSpan.FromMinutes(30) });
        var session = new Session
        {
            Token = "tok",
            UserId = 1,
            ExpiresAt = b.Time.GetUtcNow().AddMinutes(15),
            User = new User { Id = 1, Email = "a@b.c", Name = "A" }
        };
        b.Sessions.GetByTokenAsync("tok", Arg.Any<CancellationToken>()).Returns(session);

        var result = await b.Service.GetAndExtendSessionAsync("tok");

        Assert.NotNull(result);
        Assert.Equal(b.Time.GetUtcNow() + TimeSpan.FromMinutes(30), result!.ExpiresAt);
        await b.Sessions.Received(1).UpdateActivityAsync(session, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAndExtendSessionAsync_returns_null_and_deletes_when_expired()
    {
        var b = BuildService();
        var session = new Session
        {
            Token = "old",
            UserId = 1,
            ExpiresAt = b.Time.GetUtcNow().AddMinutes(-1),
            User = new User { Id = 1, Email = "a@b.c", Name = "A" }
        };
        b.Sessions.GetByTokenAsync("old", Arg.Any<CancellationToken>()).Returns(session);

        var result = await b.Service.GetAndExtendSessionAsync("old");

        Assert.Null(result);
        await b.Sessions.Received(1).DeleteAsync("old", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAndExtendSessionAsync_returns_null_for_blank_or_missing_token()
    {
        var b = BuildService();
        Assert.Null(await b.Service.GetAndExtendSessionAsync(""));

        b.Sessions.GetByTokenAsync("not-found", Arg.Any<CancellationToken>()).Returns((Session?)null);
        Assert.Null(await b.Service.GetAndExtendSessionAsync("not-found"));
    }

    [Fact]
    public async Task RequestPasswordResetAsync_silently_no_ops_for_unknown_email()
    {
        var b = BuildService();
        b.Users.GetByEmailAsync("ghost@example.com", Arg.Any<CancellationToken>()).Returns((User?)null);

        await b.Service.RequestPasswordResetAsync("ghost@example.com");

        await b.Email.DidNotReceive().SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
        await b.ResetTokens.DidNotReceive().AddAsync(Arg.Any<PasswordResetToken>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RequestPasswordResetAsync_persists_token_and_emails_known_user()
    {
        var b = BuildService(new AuthOptions { PasswordResetUrlTemplate = "https://example.com/reset?t={token}" });
        var user = new User { Id = 7, Email = "ada@example.com", Name = "Ada" };
        b.Users.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        b.Tokens.Generate(Arg.Any<int>()).Returns("reset-tok");

        await b.Service.RequestPasswordResetAsync(user.Email);

        await b.ResetTokens.Received(1).DeleteForUserAsync(user.Id, Arg.Any<CancellationToken>());
        await b.ResetTokens.Received(1).AddAsync(Arg.Is<PasswordResetToken>(t =>
            t.Token == "reset-tok" && t.UserId == user.Id), Arg.Any<CancellationToken>());
        await b.Email.Received(1).SendAsync(Arg.Is<EmailMessage>(m =>
            m.To == "ada@example.com" &&
            m.Subject.Contains("Reset") &&
            (m.BodyHtml ?? string.Empty).Contains("reset-tok")), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CompletePasswordResetAsync_returns_false_for_unknown_token()
    {
        var b = BuildService();
        b.ResetTokens.GetByTokenAsync("nope", Arg.Any<CancellationToken>()).Returns((PasswordResetToken?)null);

        Assert.False(await b.Service.CompletePasswordResetAsync("nope", "newPassword1"));
    }

    [Fact]
    public async Task CompletePasswordResetAsync_returns_false_for_used_token()
    {
        var b = BuildService();
        var user = new User { Id = 1 };
        var token = new PasswordResetToken
        {
            Token = "t",
            UserId = 1,
            User = user,
            ExpiresAt = b.Time.GetUtcNow().AddHours(1),
            UsedAt = b.Time.GetUtcNow()
        };
        b.ResetTokens.GetByTokenAsync("t", Arg.Any<CancellationToken>()).Returns(token);

        Assert.False(await b.Service.CompletePasswordResetAsync("t", "newPassword1"));
    }

    [Fact]
    public async Task CompletePasswordResetAsync_returns_false_for_expired_token()
    {
        var b = BuildService();
        var user = new User { Id = 1 };
        var token = new PasswordResetToken
        {
            Token = "t",
            UserId = 1,
            User = user,
            ExpiresAt = b.Time.GetUtcNow().AddMinutes(-1)
        };
        b.ResetTokens.GetByTokenAsync("t", Arg.Any<CancellationToken>()).Returns(token);

        Assert.False(await b.Service.CompletePasswordResetAsync("t", "newPassword1"));
    }

    [Fact]
    public async Task CompletePasswordResetAsync_updates_password_hash_and_marks_used()
    {
        var b = BuildService();
        var user = new User { Id = 1, Email = "a@b.c", Name = "A", PasswordHash = "old" };
        var token = new PasswordResetToken
        {
            Token = "t",
            UserId = 1,
            User = user,
            ExpiresAt = b.Time.GetUtcNow().AddHours(1)
        };
        b.ResetTokens.GetByTokenAsync("t", Arg.Any<CancellationToken>()).Returns(token);
        b.Hasher.HashPassword(user, "newPassword1").Returns("new-hash");

        var ok = await b.Service.CompletePasswordResetAsync("t", "newPassword1");

        Assert.True(ok);
        await b.Users.Received(1).UpdateAsync(Arg.Is<User>(u => u.PasswordHash == "new-hash"), Arg.Any<CancellationToken>());
        await b.ResetTokens.Received(1).MarkUsedAsync(Arg.Is<PasswordResetToken>(t => t.UsedAt != null), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CompletePasswordResetAsync_throws_for_short_password()
    {
        var b = BuildService();
        var user = new User { Id = 1 };
        var token = new PasswordResetToken
        {
            Token = "t",
            User = user,
            ExpiresAt = b.Time.GetUtcNow().AddHours(1)
        };
        b.ResetTokens.GetByTokenAsync("t", Arg.Any<CancellationToken>()).Returns(token);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            b.Service.CompletePasswordResetAsync("t", "short"));
    }

    [Fact]
    public async Task SignInExternalAsync_creates_user_and_links_when_external_id_is_new()
    {
        var b = BuildService();
        b.Users.GetByExternalLoginAsync("Google", "g123", Arg.Any<CancellationToken>()).Returns((User?)null);
        b.Users.GetByEmailAsync("ada@example.com", Arg.Any<CancellationToken>()).Returns((User?)null);
        b.Tokens.Generate(Arg.Any<int>()).Returns("ext-tok");

        var result = await b.Service.SignInExternalAsync("Google", "g123", "ada@example.com", "Ada");

        Assert.Equal("ada@example.com", result.User.Email);
        await b.Users.Received(1).AddExternalLoginAsync(Arg.Is<UserExternalLogin>(l =>
            l.Provider == "Google" && l.ProviderUserId == "g123"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SignInExternalAsync_links_to_existing_user_with_matching_email()
    {
        var b = BuildService();
        var user = new User { Id = 9, Email = "ada@example.com", Name = "Ada" };
        b.Users.GetByExternalLoginAsync("Google", "g123", Arg.Any<CancellationToken>()).Returns((User?)null);
        b.Users.GetByEmailAsync("ada@example.com", Arg.Any<CancellationToken>()).Returns(user);
        b.Tokens.Generate(Arg.Any<int>()).Returns("ext-tok");

        var result = await b.Service.SignInExternalAsync("Google", "g123", "ada@example.com", "Ada");

        Assert.Equal(9, result.User.Id);
        await b.Users.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await b.Users.Received(1).AddExternalLoginAsync(Arg.Is<UserExternalLogin>(l =>
            l.UserId == 9 && l.Provider == "Google"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SignInExternalAsync_returns_session_for_already_linked_user()
    {
        var b = BuildService();
        var user = new User { Id = 9, Email = "ada@example.com", Name = "Ada" };
        b.Users.GetByExternalLoginAsync("Google", "g123", Arg.Any<CancellationToken>()).Returns(user);
        b.Tokens.Generate(Arg.Any<int>()).Returns("ext-tok");

        var result = await b.Service.SignInExternalAsync("Google", "g123", "ada@example.com", "Ada");

        Assert.Equal(9, result.User.Id);
        await b.Users.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await b.Users.DidNotReceive().AddExternalLoginAsync(Arg.Any<UserExternalLogin>(), Arg.Any<CancellationToken>());
    }
}

internal class FakeTimeProvider : TimeProvider
{
    private DateTimeOffset _now;
    public FakeTimeProvider(DateTimeOffset now) { _now = now; }
    public override DateTimeOffset GetUtcNow() => _now;
    public void Advance(TimeSpan span) => _now += span;
}
