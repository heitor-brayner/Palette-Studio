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
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // Play the ConnectedAnimation to the preview rectangle.
        // REASON: TryStart returns false gracefully if no animation was prepared
        // (e.g. user navigated via keyboard), so this is safe to call unconditionally.
        // Using Loaded to ensure the element is in the visual tree before starting.
        Loaded += OnPageLoaded;
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnPageLoaded;
        var anim = ConnectedAnimationService.GetForCurrentView().GetAnimation("SwatchToDetail");
        anim?.TryStart(ColorPreviewRect);
    }
}
