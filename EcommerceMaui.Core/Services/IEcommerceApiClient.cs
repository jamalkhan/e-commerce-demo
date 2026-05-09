using EcommerceMaui.Models;

namespace EcommerceMaui.Services;

public interface IEcommerceApiClient
{
    Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetProductAsync(int id, CancellationToken cancellationToken = default);
    Task<SearchResponse> SearchAsync(string query, CancellationToken cancellationToken = default);
    Task<LoginResponse> LoginAsync(string userName, CancellationToken cancellationToken = default);
}
