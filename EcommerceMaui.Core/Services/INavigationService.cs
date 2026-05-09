namespace EcommerceMaui.Services;

public interface INavigationService
{
    Task GoToAsync(string route);
    Task GoBackAsync();
    Task GoHomeAsync();
}
