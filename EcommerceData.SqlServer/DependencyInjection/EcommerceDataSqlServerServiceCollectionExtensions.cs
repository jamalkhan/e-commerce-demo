using EcommerceData.Data;
using EcommerceData.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EcommerceData.SqlServer.DependencyInjection;

public static class EcommerceDataSqlServerServiceCollectionExtensions
{
    public static IServiceCollection AddSqlServerEcommerceData(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<EcommerceDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(typeof(EcommerceDataSqlServerServiceCollectionExtensions).Assembly.FullName)));

        services.AddEcommerceDataRepositories();

        return services;
    }
}
