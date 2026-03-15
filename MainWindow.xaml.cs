using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using PaletteStudio.Contracts;
using PaletteStudio.Views;
using Windows.Graphics;
using WinRT.Interop;

namespace PaletteStudio;

public sealed partial class MainWindow : Window
{
    private readonly INavigationService _navigationService;
    private readonly IThemeService _themeService;

    public MainWindow(INavigationService navigationService, IThemeService themeService)
    {
        _navigationService = navigationService;
        _themeService = themeService;
        InitializeComponent();

        SetupTitleBar();
        SetupMica();
        _themeService.Initialize((FrameworkElement)Content);
        _navigationService.SetFrame(ContentFrame);

        // Select first nav item and navigate on load
        NavView.Loaded += (_, _) =>
        {
            NavView.SelectedItem = PaletteNavItem;
            _navigationService.NavigateTo(nameof(PalettePage));
        };
    }

    private void SetupTitleBar()
    {
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        // Keep left/right padding columns in sync with caption button sizes
        var appWindow = GetAppWindow();
        if (appWindow?.TitleBar is { } tb)
        {
            tb.ButtonBackgroundColor = Colors.Transparent;
            tb.ButtonInactiveBackgroundColor = Colors.Transparent;

            AppTitleBar.Loaded += (_, _) => UpdateTitleBarLayout(tb);
            AppTitleBar.SizeChanged += (_, _) => UpdateTitleBarLayout(tb);
        }
    }

    private void UpdateTitleBarLayout(AppWindowTitleBar tb)
    {
        LeftPaddingColumn.Width = new GridLength(tb.LeftInset);
        RightPaddingColumn.Width = new GridLength(tb.RightInset);
    }

    private void SetupMica()
    {
        // REASON: MicaBackdrop requires Windows 11 (build 22000+); on older builds
        // we fall back to DesktopAcrylic, and on very old builds the backdrop stays null.
        if (Microsoft.UI.Composition.SystemBackdrops.MicaController.IsSupported())
        {
            SystemBackdrop = new MicaBackdrop();
        }
        else if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
        {
            SystemBackdrop = new DesktopAcrylicBackdrop();
        }
    }

    private AppWindow GetAppWindow()
    {
        var hWnd = WindowNative.GetWindowHandle(this);
        var wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        return AppWindow.GetFromWindowId(wndId);
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item && item.Tag is string tag)
        {
            _navigationService.NavigateTo(tag);
        }
    }
}
