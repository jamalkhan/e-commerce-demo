using EcommerceData.Data;
using EcommerceData.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceData.Repositories;

public class EfSessionRepository : ISessionRepository
{
    private readonly EcommerceDbContext _db;

    public EfSessionRepository(EcommerceDbContext db)
    {
        _db = db;
    }

    public Task<Session?> GetByTokenAsync(string token, CancellationToken cancellationToken = default) =>
        _db.Sessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Token == token, cancellationToken);

    public async Task<Session> AddAsync(Session session, CancellationToken cancellationToken = default)
    {
        _db.Sessions.Add(session);
        await _db.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task UpdateActivityAsync(Session session, CancellationToken cancellationToken = default)
    {
        _db.Sessions.Update(session);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(string token, CancellationToken cancellationToken = default)
    {
        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Token == token, cancellationToken);
        if (session is null) return;
        _db.Sessions.Remove(session);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteExpiredAsync(DateTimeOffset asOf, CancellationToken cancellationToken = default)
    {
        var expired = await _db.Sessions.Where(s => s.ExpiresAt <= asOf).ToListAsync(cancellationToken);
        if (expired.Count == 0) return;
        _db.Sessions.RemoveRange(expired);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
