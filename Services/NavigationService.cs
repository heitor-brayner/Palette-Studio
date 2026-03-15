using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using PaletteStudio.Contracts;
using PaletteStudio.Views;

namespace PaletteStudio.Services;

public sealed class NavigationService : INavigationService
{
    private static readonly Dictionary<string, Type> _pages = new()
    {
        [nameof(PalettePage)]     = typeof(PalettePage),
        [nameof(ColorDetailPage)] = typeof(ColorDetailPage),
        [nameof(ExportPage)]      = typeof(ExportPage),
        [nameof(SettingsPage)]    = typeof(SettingsPage),
    };

    private Frame? _frame;

    public bool CanGoBack => _frame?.CanGoBack ?? false;

    public event EventHandler<string>? Navigated;

    public void SetFrame(Frame frame)
    {
        _frame = frame;
    }

    public bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false)
    {
        if (_frame is null) return false;
        if (!_pages.TryGetValue(pageKey, out var pageType)) return false;

        // Avoid duplicate navigation to the same page (unless it's detail page)
        if (_frame.Content?.GetType() == pageType && pageType != typeof(ColorDetailPage))
            return false;

        var navigated = _frame.Navigate(pageType, parameter, new EntranceNavigationTransitionInfo());
        if (navigated && clearNavigation)
            _frame.BackStack.Clear();

        if (navigated)
            Navigated?.Invoke(this, pageKey);

        return navigated;
    }

    public bool GoBack()
    {
        if (_frame?.CanGoBack != true) return false;
        _frame.GoBack();
        return true;
    }
}
