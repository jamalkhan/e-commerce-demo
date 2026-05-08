using EcommerceMaui.ViewModels;

namespace EcommerceMaui.Views;

public partial class NotFoundPage : ContentPage
{
    public NotFoundPage(NotFoundViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
