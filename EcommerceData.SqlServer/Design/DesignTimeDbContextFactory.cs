using EcommerceData.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EcommerceData.SqlServer.Design;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<EcommerceDbContext>
{
    public EcommerceDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ECOMMERCE_DESIGN_CONNECTIONSTRING")
            ?? "Server=localhost,1433;Database=Ecommerce;User Id=sa;Password=__designtime__;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<EcommerceDbContext>()
            .UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.FullName))
            .Options;

        return new EcommerceDbContext(options);
    }
}
