using EcommerceData.Entities;

namespace EcommerceData.Repositories;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<PasswordResetToken> AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
    Task MarkUsedAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
    Task DeleteForUserAsync(int userId, CancellationToken cancellationToken = default);
}
