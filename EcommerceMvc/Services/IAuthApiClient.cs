namespace EcommerceMvc.Services;

public interface IAuthApiClient
{
    Task<AuthApiResult<AuthApiSession>> RegisterAsync(string email, string name, string password, CancellationToken cancellationToken = default);
    Task<AuthApiResult<AuthApiSession>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task LogoutAsync(string sessionToken, CancellationToken cancellationToken = default);
    Task<AuthApiResult<AuthApiUser>> GetCurrentUserAsync(string sessionToken, CancellationToken cancellationToken = default);
    Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);
    Task<AuthApiResult<bool>> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default);
}
