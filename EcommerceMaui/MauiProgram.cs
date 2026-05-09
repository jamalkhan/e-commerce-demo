using bleak.Api.Rest;
using EcommerceMaui.Services;
using EcommerceMaui.ViewModels;
using EcommerceMaui.Views;
using Microsoft.Extensions.Logging;

namespace EcommerceMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<RestClient>();
        builder.Services.AddSingleton<IEcommerceApiClient, EcommerceApiClient>();
        builder.Services.AddSingleton<INavigationService, ShellNavigationService>();

        builder.Services.AddSingleton<AppShell>();

        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<ProductsViewModel>();
        builder.Services.AddTransient<ProductDetailsViewModel>();
        builder.Services.AddTransient<SearchViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<PrivacyViewModel>();
        builder.Services.AddTransient<NotFoundViewModel>();

        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<ProductsPage>();
        builder.Services.AddTransient<ProductDetailsPage>();
        builder.Services.AddTransient<SearchPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<PrivacyPage>();
        builder.Services.AddTransient<NotFoundPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
