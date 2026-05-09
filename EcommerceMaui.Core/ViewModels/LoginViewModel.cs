using System.Windows.Input;
using EcommerceMaui.Services;

namespace EcommerceMaui.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly IEcommerceApiClient _api;
    private readonly INavigationService _navigation;
    private string _userName = string.Empty;
    private string? _welcomeMessage;

    public LoginViewModel(IEcommerceApiClient api, INavigationService navigation)
    {
        _api = api;
        _navigation = navigation;
        Title = "Login";
        BackCommand = new RelayCommand(BackAsync);
    }

    public string UserName
    {
        get => _userName;
        set
        {
            if (SetProperty(ref _userName, value))
            {
                _ = LoginAsync();
            }
        }
    }

    public string? WelcomeMessage
    {
        get => _welcomeMessage;
        private set => SetProperty(ref _welcomeMessage, value);
    }

    public ICommand BackCommand { get; }

    public async Task LoginAsync()
    {
        if (IsBusy || string.IsNullOrWhiteSpace(UserName)) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            WelcomeMessage = null;

            var response = await _api.LoginAsync(UserName);
            UserName = response.UserName;
            WelcomeMessage = response.Message;
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

    private Task BackAsync() => _navigation.GoHomeAsync();
}
