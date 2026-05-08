namespace EcommerceMaui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = IPlatformApplication.Current?.Services.GetService<AppShell>() ?? new AppShell();
        return new Window(shell);
    }
}
