using EcommerceData.Entities;

namespace EcommerceData.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByExternalLoginAsync(string provider, string providerUserId, CancellationToken cancellationToken = default);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<UserExternalLogin> AddExternalLoginAsync(UserExternalLogin login, CancellationToken cancellationToken = default);
}
