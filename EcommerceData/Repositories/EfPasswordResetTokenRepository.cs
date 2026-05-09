using EcommerceData.Data;
using EcommerceData.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceData.Repositories;

public class EfPasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly EcommerceDbContext _db;

    public EfPasswordResetTokenRepository(EcommerceDbContext db)
    {
        _db = db;
    }

    public Task<PasswordResetToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default) =>
        _db.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);

    public async Task<PasswordResetToken> AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
    {
        _db.PasswordResetTokens.Add(token);
        await _db.SaveChangesAsync(cancellationToken);
        return token;
    }

    public async Task MarkUsedAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
    {
        _db.PasswordResetTokens.Update(token);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var existing = await _db.PasswordResetTokens.Where(t => t.UserId == userId).ToListAsync(cancellationToken);
        if (existing.Count == 0) return;
        _db.PasswordResetTokens.RemoveRange(existing);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
