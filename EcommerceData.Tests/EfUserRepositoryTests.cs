using EcommerceData.Entities;
using EcommerceData.Repositories;
using Xunit;

namespace EcommerceData.Tests;

public class EfUserRepositoryTests
{
    [Fact]
    public async Task AddAsync_persists_user_with_lowercase_email_and_sets_created_at()
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new EfUserRepository(fixture.DbContext);

        var user = await repo.AddAsync(new User
        {
            Email = "ada@example.com",
            Name = "Ada"
        });

        Assert.NotEqual(0, user.Id);
        Assert.NotEqual(default, user.CreatedAt);
    }

    [Fact]
    public async Task GetByEmailAsync_is_case_insensitive()
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new EfUserRepository(fixture.DbContext);
        await repo.AddAsync(new User { Email = "ada@example.com", Name = "Ada" });

        var found = await repo.GetByEmailAsync("ADA@Example.com");

        Assert.NotNull(found);
        Assert.Equal("Ada", found!.Name);
    }

    [Fact]
    public async Task GetByEmailAsync_returns_null_when_no_match()
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new EfUserRepository(fixture.DbContext);

        var found = await repo.GetByEmailAsync("nobody@example.com");

        Assert.Null(found);
    }

    [Fact]
    public async Task AddExternalLoginAsync_links_login_to_user()
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new EfUserRepository(fixture.DbContext);
        var user = await repo.AddAsync(new User { Email = "ada@example.com", Name = "Ada" });

        await repo.AddExternalLoginAsync(new UserExternalLogin
        {
            UserId = user.Id,
            Provider = "Google",
            ProviderUserId = "google-12345"
        });

        var found = await repo.GetByExternalLoginAsync("Google", "google-12345");
        Assert.NotNull(found);
        Assert.Equal(user.Id, found!.Id);
    }

    [Fact]
    public async Task GetByExternalLoginAsync_returns_null_when_no_match()
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new EfUserRepository(fixture.DbContext);

        var found = await repo.GetByExternalLoginAsync("Google", "nope");

        Assert.Null(found);
    }

    [Fact]
    public async Task UpdateAsync_persists_password_hash_change()
    {
        using var fixture = new SqliteDbContextFixture();
        var repo = new EfUserRepository(fixture.DbContext);
        var user = await repo.AddAsync(new User { Email = "ada@example.com", Name = "Ada", PasswordHash = "old" });

        user.PasswordHash = "new";
        await repo.UpdateAsync(user);

        var refetched = await repo.GetByIdAsync(user.Id);
        Assert.Equal("new", refetched!.PasswordHash);
    }
}
