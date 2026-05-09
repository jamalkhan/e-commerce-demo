using System.Windows.Input;
using EcommerceMaui.Models;
using EcommerceMaui.Services;

namespace EcommerceMaui.ViewModels;

public class ProductDetailsViewModel : BaseViewModel
{
    private readonly IEcommerceApiClient _api;
    private readonly INavigationService _navigation;
    private int _id;
    private Product? _product;
    private bool _notFound;

    public ProductDetailsViewModel(IEcommerceApiClient api, INavigationService navigation)
    {
        _api = api;
        _navigation = navigation;
        Title = "Product Details";
        BackCommand = new RelayCommand(BackAsync);
    }

    public int Id
    {
        get => _id;
        set
        {
            if (SetProperty(ref _id, value))
            {
                _ = LoadAsync();
            }
        }
    }

    public Product? Product
    {
        get => _product;
        private set => SetProperty(ref _product, value);
    }

    public bool NotFound
    {
        get => _notFound;
        private set => SetProperty(ref _notFound, value);
    }

    public ICommand BackCommand { get; }

    public async Task LoadAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            NotFound = false;
            Product = null;

            var product = await _api.GetProductAsync(Id);
            if (product is null)
            {
                NotFound = true;
                return;
            }

            Product = product;
            Title = product.Name;
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

    private Task BackAsync() => _navigation.GoBackAsync();
}
