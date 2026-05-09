using EcommerceData.Entities;
using EcommerceData.Repositories;
using Xunit;

namespace EcommerceData.Tests;

public class EfPasswordResetTokenRepositoryTests
{
    [Fact]
    public async Task AddAsync_then_GetByTokenAsync_round_trips_with_user_loaded()
    {
        using var fixture = new SqliteDbContextFixture();
        var users = new EfUserRepository(fixture.DbContext);
        var user = await users.AddAsync(new User { Email = "a@b.c", Name = "A" });
        var repo = new EfPasswordResetTokenRepository(fixture.DbContext);

        await repo.AddAsync(new PasswordResetToken
        {
            Token = "rst",
            UserId = user.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        });

        var found = await repo.GetByTokenAsync("rst");
        Assert.NotNull(found);
        Assert.NotNull(found!.User);
        Assert.Equal("a@b.c", found.User!.Email);
    }

    [Fact]
    public async Task MarkUsedAsync_persists_used_at_timestamp()
    {
        using var fixture = new SqliteDbContextFixture();
        var users = new EfUserRepository(fixture.DbContext);
        var user = await users.AddAsync(new User { Email = "a@b.c", Name = "A" });
        var repo = new EfPasswordResetTokenRepository(fixture.DbContext);
        var token = await repo.AddAsync(new PasswordResetToken
        {
            Token = "rst",
            UserId = user.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        });

        token.UsedAt = DateTimeOffset.UtcNow;
        await repo.MarkUsedAsync(token);

        var refetched = await repo.GetByTokenAsync("rst");
        Assert.NotNull(refetched!.UsedAt);
    }

    [Fact]
    public async Task DeleteForUserAsync_removes_all_tokens_for_user()
    {
        using var fixture = new SqliteDbContextFixture();
        var users = new EfUserRepository(fixture.DbContext);
        var user = await users.AddAsync(new User { Email = "a@b.c", Name = "A" });
        var repo = new EfPasswordResetTokenRepository(fixture.DbContext);
        await repo.AddAsync(new PasswordResetToken { Token = "t1", UserId = user.Id, CreatedAt = DateTimeOffset.UtcNow, ExpiresAt = DateTimeOffset.UtcNow.AddHours(1) });
        await repo.AddAsync(new PasswordResetToken { Token = "t2", UserId = user.Id, CreatedAt = DateTimeOffset.UtcNow, ExpiresAt = DateTimeOffset.UtcNow.AddHours(1) });

        await repo.DeleteForUserAsync(user.Id);

        Assert.Null(await repo.GetByTokenAsync("t1"));
        Assert.Null(await repo.GetByTokenAsync("t2"));
    }
}
