using EcommerceMaui.ViewModels;

namespace EcommerceMaui.Views;

public partial class LoginPage : ContentPage, IQueryAttributable
{
    private readonly LoginViewModel _viewModel;

    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("uname", out var raw))
        {
            _viewModel.UserName = Uri.UnescapeDataString(raw?.ToString() ?? string.Empty);
        }
    }
}
