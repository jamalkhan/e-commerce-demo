using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EcommerceMail.Smtp.DependencyInjection;

public static class EcommerceMailSmtpServiceCollectionExtensions
{
    public static IServiceCollection AddSmtpEmail(this IServiceCollection services, IConfiguration configurationSection)
    {
        services.Configure<SmtpOptions>(configurationSection);
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        return services;
    }

    public static IServiceCollection AddSmtpEmail(this IServiceCollection services, Action<SmtpOptions> configure)
    {
        services.Configure(configure);
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        return services;
    }
}
