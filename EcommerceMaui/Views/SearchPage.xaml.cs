using EcommerceMaui.ViewModels;

namespace EcommerceMaui.Views;

public partial class SearchPage : ContentPage, IQueryAttributable
{
    private readonly SearchViewModel _viewModel;

    public SearchPage(SearchViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("q", out var raw))
        {
            _viewModel.Query = Uri.UnescapeDataString(raw?.ToString() ?? string.Empty);
        }
    }
}
