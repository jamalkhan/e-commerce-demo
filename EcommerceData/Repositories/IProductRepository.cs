using EcommerceData.Entities;

namespace EcommerceData.Repositories;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> SearchByNameAsync(string query, CancellationToken cancellationToken = default);
}
