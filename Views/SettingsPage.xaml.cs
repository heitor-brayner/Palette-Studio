using Microsoft.UI.Xaml.Controls;
using PaletteStudio.ViewModels;

namespace PaletteStudio.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
    }
}
