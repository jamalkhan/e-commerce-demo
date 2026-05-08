using EcommerceMaui.Views;

namespace EcommerceMaui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("search", typeof(SearchPage));
        Routing.RegisterRoute("product", typeof(ProductDetailsPage));
        Routing.RegisterRoute("login", typeof(LoginPage));
        Routing.RegisterRoute("notfound", typeof(NotFoundPage));
    }
}
