using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using PaletteStudio.Models;
using PaletteStudio.ViewModels;

namespace PaletteStudio.Views;

public sealed partial class ColorDetailPage : Page
{
    public ColorDetailViewModel ViewModel { get; }

    public ColorDetailPage()
    {
        ViewModel = App.GetService<ColorDetailViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is ColorModel color)
            ViewModel.LoadColor(color);

        // Defer the ConnectedAnimation until the element is in the visual tree.
        // REASON: TryStart fails silently if the target element is not yet rendered.
        Loaded += OnPageLoaded;
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnPageLoaded;
        var anim = ConnectedAnimationService.GetForCurrentView().GetAnimation("SwatchToDetail");
        anim?.TryStart(ColorPreviewRect);
    }
}
