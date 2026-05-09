using EcommerceData.Repositories;
using EcommerceData.Seeding;
using Microsoft.Extensions.DependencyInjection;

namespace EcommerceData.DependencyInjection;

public static class EcommerceDataServiceCollectionExtensions
{
    public static IServiceCollection AddEcommerceDataRepositories(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, EfProductRepository>();
        services.AddScoped<ProductSeeder>();
        return services;
    }
}
