using EcommerceMaui.ViewModels;

namespace EcommerceMaui.Views;

public partial class ProductDetailsPage : ContentPage, IQueryAttributable
{
    private readonly ProductDetailsViewModel _viewModel;

    public ProductDetailsPage(ProductDetailsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var raw) &&
            int.TryParse(raw?.ToString(), out var id))
        {
            _viewModel.Id = id;
        }
    }
}
