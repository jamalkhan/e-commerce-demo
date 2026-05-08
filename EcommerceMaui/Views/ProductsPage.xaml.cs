using EcommerceMaui.ViewModels;

namespace EcommerceMaui.Views;

public partial class ProductsPage : ContentPage
{
    private readonly ProductsViewModel _viewModel;

    public ProductsPage(ProductsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.Products.Count == 0)
        {
            await _viewModel.LoadAsync();
        }
    }
}
