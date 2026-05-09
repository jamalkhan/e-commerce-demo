using System.Windows.Input;
using EcommerceMaui.Services;

namespace EcommerceMaui.ViewModels;

public class HomeViewModel : BaseViewModel
{
    private readonly INavigationService _navigation;
    private string _searchQuery = string.Empty;
    private string _userName = string.Empty;

    public HomeViewModel(INavigationService navigation)
    {
        _navigation = navigation;
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
        await _navigation.GoToAsync($"search?q={Uri.EscapeDataString(q)}");
    }

    private async Task LoginAsync()
    {
        var name = UserName.Trim();
        if (string.IsNullOrEmpty(name)) return;
        await _navigation.GoToAsync($"login?uname={Uri.EscapeDataString(name)}");
    }
}
