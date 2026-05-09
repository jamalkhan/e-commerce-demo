using EcommerceData.Entities;
using EcommerceData.Repositories;
using Xunit;

namespace EcommerceData.Tests;

public class EfSessionRepositoryTests
{
    [Fact]
    public async Task AddAsync_persists_session_with_token_as_key()
    {
        using var fixture = new SqliteDbContextFixture();
        var users = new EfUserRepository(fixture.DbContext);
        var user = await users.AddAsync(new User { Email = "a@b.c", Name = "A" });
        var sessions = new EfSessionRepository(fixture.DbContext);

        var session = await sessions.AddAsync(new Session
        {
            Token = "abc123",
            UserId = user.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            LastActivityAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30)
        });

        var found = await sessions.GetByTokenAsync("abc123");
        Assert.NotNull(found);
        Assert.Equal(user.Id, found!.UserId);
        Assert.NotNull(found.User);
        Assert.Equal("a@b.c", found.User!.Email);
    }

    [Fact]
    public async Task DeleteAsync_removes_session()
    {
        using var fixture = new SqliteDbContextFixture();
        var users = new EfUserRepository(fixture.DbContext);
        var user = await users.AddAsync(new User { Email = "a@b.c", Name = "A" });
        var sessions = new EfSessionRepository(fixture.DbContext);
        await sessions.AddAsync(new Session
        {
            Token = "tok",
            UserId = user.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            LastActivityAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30)
        });

        await sessions.DeleteAsync("tok");

        Assert.Null(await sessions.GetByTokenAsync("tok"));
    }

    [Fact]
    public async Task DeleteAsync_does_not_throw_when_token_unknown()
    {
        using var fixture = new SqliteDbContextFixture();
        var sessions = new EfSessionRepository(fixture.DbContext);

        var ex = await Record.ExceptionAsync(() => sessions.DeleteAsync("missing"));
        Assert.Null(ex);
    }

    [Fact]
    public async Task DeleteExpiredAsync_removes_only_sessions_expired_before_cutoff()
    {
        using var fixture = new SqliteDbContextFixture();
        var users = new EfUserRepository(fixture.DbContext);
        var user = await users.AddAsync(new User { Email = "a@b.c", Name = "A" });
        var sessions = new EfSessionRepository(fixture.DbContext);
        var now = DateTimeOffset.UtcNow;

        await sessions.AddAsync(new Session
        {
            Token = "expired",
            UserId = user.Id,
            CreatedAt = now.AddHours(-2),
            LastActivityAt = now.AddHours(-2),
            ExpiresAt = now.AddMinutes(-10)
        });
        await sessions.AddAsync(new Session
        {
            Token = "active",
            UserId = user.Id,
            CreatedAt = now,
            LastActivityAt = now,
            ExpiresAt = now.AddMinutes(30)
        });

        await sessions.DeleteExpiredAsync(now);

        Assert.Null(await sessions.GetByTokenAsync("expired"));
        Assert.NotNull(await sessions.GetByTokenAsync("active"));
    }
}
