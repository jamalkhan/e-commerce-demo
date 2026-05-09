using EcommerceData.Entities;

namespace EcommerceData.Repositories;

public interface ISessionRepository
{
    Task<Session?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<Session> AddAsync(Session session, CancellationToken cancellationToken = default);
    Task UpdateActivityAsync(Session session, CancellationToken cancellationToken = default);
    Task DeleteAsync(string token, CancellationToken cancellationToken = default);
    Task DeleteExpiredAsync(DateTimeOffset asOf, CancellationToken cancellationToken = default);
}
