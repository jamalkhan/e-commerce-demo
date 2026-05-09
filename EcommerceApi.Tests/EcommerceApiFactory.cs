using EcommerceData.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EcommerceApi.Tests;

public class EcommerceApiFactory : WebApplicationFactory<Program>
{
    public IProductRepository ProductRepository { get; }

    public EcommerceApiFactory(IProductRepository productRepository)
    {
        ProductRepository = productRepository;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Database:Provider", "SqlServer");
        builder.UseSetting("ConnectionStrings:EcommerceDb",
            "Server=test;Database=test;User Id=sa;Password=test;TrustServerCertificate=True;");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IProductRepository>();
            services.AddSingleton(ProductRepository);
        });
    }
}
