using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PaletteStudio.ViewModels;

namespace PaletteStudio.Views;

public sealed partial class ExportPage : Page
{
    public ExportViewModel ViewModel { get; }

    public ExportPage()
    {
        ViewModel = App.GetService<ExportViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // Sync palette from PaletteViewModel each time this page becomes visible
        var paletteVm = App.GetService<PaletteViewModel>();
        ViewModel.LoadColors(paletteVm.Colors);
    }
}
