using EcommerceApi.Services;
using EcommerceData.Repositories;
using EcommerceMail;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EcommerceApi.Tests;

public class EcommerceApiFactory : WebApplicationFactory<Program>
{
    public IProductRepository? ProductRepository { get; set; }
    public IAuthenticationService? AuthenticationService { get; set; }
    public IEmailSender? EmailSender { get; set; }

    public EcommerceApiFactory() { }

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
        builder.UseSetting("Smtp:Host", "localhost");
        builder.UseSetting("Smtp:Port", "25");

        builder.ConfigureServices(services =>
        {
            if (ProductRepository is not null)
            {
                services.RemoveAll<IProductRepository>();
                services.AddSingleton(ProductRepository);
            }

            if (AuthenticationService is not null)
            {
                services.RemoveAll<IAuthenticationService>();
                services.AddSingleton(AuthenticationService);
            }

            if (EmailSender is not null)
            {
                services.RemoveAll<IEmailSender>();
                services.AddSingleton(EmailSender);
            }
        });
    }
}
