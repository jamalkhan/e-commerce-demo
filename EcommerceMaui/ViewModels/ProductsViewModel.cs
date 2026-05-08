using System.Collections.ObjectModel;
using System.Windows.Input;
using EcommerceMaui.Models;
using EcommerceMaui.Services;

namespace EcommerceMaui.ViewModels;

public class ProductsViewModel : BaseViewModel
{
    private readonly IEcommerceApiClient _api;

    public ProductsViewModel(IEcommerceApiClient api)
    {
        _api = api;
        Title = "Products";
        Products = new ObservableCollection<Product>();
        LoadCommand = new RelayCommand(LoadAsync);
        ViewDetailsCommand = new RelayCommand<Product>(ViewDetailsAsync);
    }

    public ObservableCollection<Product> Products { get; }

    public ICommand LoadCommand { get; }
    public ICommand ViewDetailsCommand { get; }

    public async Task LoadAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            Products.Clear();

            var products = await _api.GetProductsAsync();
            foreach (var p in products)
            {
                Products.Add(p);
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
        await Shell.Current.GoToAsync($"product?id={product.Id}");
    }
}
