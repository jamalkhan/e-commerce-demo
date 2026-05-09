using System.Windows.Input;
using EcommerceMaui.Services;

namespace EcommerceMaui.ViewModels;

public class NotFoundViewModel : BaseViewModel
{
    private readonly INavigationService _navigation;

    public NotFoundViewModel(INavigationService navigation)
    {
        _navigation = navigation;
        Title = "Not Found";
        HomeCommand = new RelayCommand(HomeAsync);
    }

    public ICommand HomeCommand { get; }

    private Task HomeAsync() => _navigation.GoHomeAsync();
}
