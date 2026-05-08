using EcommerceMaui.ViewModels;

namespace EcommerceMaui.Views;

public partial class PrivacyPage : ContentPage
{
    public PrivacyPage(PrivacyViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
