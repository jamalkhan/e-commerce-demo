using EcommerceMaui.Services;

namespace EcommerceMaui.Services;

public class ShellNavigationService : INavigationService
{
    public Task GoToAsync(string route) => Shell.Current.GoToAsync(route);

    public Task GoBackAsync() => Shell.Current.GoToAsync("..");

    public Task GoHomeAsync() => Shell.Current.GoToAsync("//home");
}
