using System.Windows.Input;

namespace EcommerceMaui.ViewModels;

public class NotFoundViewModel : BaseViewModel
{
    public NotFoundViewModel()
    {
        Title = "Not Found";
        HomeCommand = new RelayCommand(HomeAsync);
    }

    public ICommand HomeCommand { get; }

    private static async Task HomeAsync()
    {
        await Shell.Current.GoToAsync("//home");
    }
}
