using EcommerceData.Data;
using EcommerceData.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceData.Repositories;

public class EfProductRepository : IProductRepository
{
    private readonly EcommerceDbContext _db;

    public EfProductRepository(EcommerceDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Products
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> SearchByNameAsync(string query, CancellationToken cancellationToken = default)
    {
        var trimmed = query?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmed))
        {
            return Array.Empty<Product>();
        }

        return await _db.Products
            .AsNoTracking()
            .Where(p => EF.Functions.Like(p.Name, $"%{trimmed}%"))
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }
}
