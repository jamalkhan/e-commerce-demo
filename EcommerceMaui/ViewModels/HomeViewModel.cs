using System.Windows.Input;

namespace EcommerceMaui.ViewModels;

public class HomeViewModel : BaseViewModel
{
    private string _searchQuery = string.Empty;
    private string _userName = string.Empty;

    public HomeViewModel()
    {
        Title = "Home";
        SearchCommand = new RelayCommand(SearchAsync, () => !string.IsNullOrWhiteSpace(SearchQuery));
        LoginCommand = new RelayCommand(LoginAsync, () => !string.IsNullOrWhiteSpace(UserName));
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
            {
                ((RelayCommand)SearchCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string UserName
    {
        get => _userName;
        set
        {
            if (SetProperty(ref _userName, value))
            {
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand SearchCommand { get; }
    public ICommand LoginCommand { get; }

    private async Task SearchAsync()
    {
        var q = SearchQuery.Trim();
        if (string.IsNullOrEmpty(q)) return;
        await Shell.Current.GoToAsync($"search?q={Uri.EscapeDataString(q)}");
    }

    private async Task LoginAsync()
    {
        var name = UserName.Trim();
        if (string.IsNullOrEmpty(name)) return;
        await Shell.Current.GoToAsync($"login?uname={Uri.EscapeDataString(name)}");
    }
}
