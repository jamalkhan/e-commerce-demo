using System.Collections.ObjectModel;
using System.Windows.Input;
using EcommerceMaui.Models;
using EcommerceMaui.Services;

namespace EcommerceMaui.ViewModels;

public class SearchViewModel : BaseViewModel
{
    private readonly IEcommerceApiClient _api;
    private readonly INavigationService _navigation;
    private string _query = string.Empty;
    private string? _resultsMessage;

    public SearchViewModel(IEcommerceApiClient api, INavigationService navigation)
    {
        _api = api;
        _navigation = navigation;
        Title = "Search";
        Results = new ObservableCollection<Product>();
        ViewDetailsCommand = new RelayCommand<Product>(ViewDetailsAsync);
        BackCommand = new RelayCommand(BackAsync);
    }

    public string Query
    {
        get => _query;
        set
        {
            if (SetProperty(ref _query, value))
            {
                _ = LoadAsync();
            }
        }
    }

    public string? ResultsMessage
    {
        get => _resultsMessage;
        private set => SetProperty(ref _resultsMessage, value);
    }

    public ObservableCollection<Product> Results { get; }

    public ICommand ViewDetailsCommand { get; }
    public ICommand BackCommand { get; }

    public async Task LoadAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            ResultsMessage = null;
            Results.Clear();

            var response = await _api.SearchAsync(Query);
            ResultsMessage = response.Message;
            foreach (var p in response.Results)
            {
                Results.Add(p);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ViewDetailsAsync(Product? product)
    {
        if (product is null) return;
        await _navigation.GoToAsync($"product?id={product.Id}");
    }

    private Task BackAsync() => _navigation.GoBackAsync();
}
