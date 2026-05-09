using EcommerceData.Data;
using EcommerceData.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceData.Repositories;

public class EfUserRepository : IUserRepository
{
    private readonly EcommerceDbContext _db;

    public EfUserRepository(EcommerceDbContext db)
    {
        _db = db;
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _db.Users
            .Include(u => u.ExternalLogins)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = (email ?? string.Empty).Trim().ToLowerInvariant();
        return _db.Users
            .Include(u => u.ExternalLogins)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalized, cancellationToken);
    }

    public async Task<User?> GetByExternalLoginAsync(string provider, string providerUserId, CancellationToken cancellationToken = default)
    {
        var login = await _db.UserExternalLogins
            .Include(l => l.User!)
            .ThenInclude(u => u.ExternalLogins)
            .FirstOrDefaultAsync(l => l.Provider == provider && l.ProviderUserId == providerUserId, cancellationToken);
        return login?.User;
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user.CreatedAt == default)
        {
            user.CreatedAt = DateTimeOffset.UtcNow;
        }
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserExternalLogin> AddExternalLoginAsync(UserExternalLogin login, CancellationToken cancellationToken = default)
    {
        if (login.CreatedAt == default)
        {
            login.CreatedAt = DateTimeOffset.UtcNow;
        }
        _db.UserExternalLogins.Add(login);
        await _db.SaveChangesAsync(cancellationToken);
        return login;
    }
}
