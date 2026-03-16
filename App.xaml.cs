using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using PaletteStudio.Contracts;
using PaletteStudio.Services;
using PaletteStudio.ViewModels;
using PaletteStudio.Views;

namespace PaletteStudio;

public partial class App : Application
{
    public static MainWindow? MainWindow { get; private set; }

    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        InitializeComponent();
        _serviceProvider = ConfigureServices();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = GetService<MainWindow>();
        MainWindow.Activate();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IColorExtractionService, ColorExtractionService>();

        // ViewModels
        // PaletteViewModel is singleton so ExportPage and ColorDetailPage read
        // the same Colors collection that PalettePage populated.
        services.AddSingleton<PaletteViewModel>();
        services.AddSingleton<ColorDetailViewModel>();
        services.AddTransient<ExportViewModel>();
        services.AddTransient<SettingsViewModel>();

        // Views (transient so each navigation creates a fresh page)
        services.AddTransient<PalettePage>();
        services.AddTransient<ColorDetailPage>();
        services.AddTransient<ExportPage>();
        services.AddTransient<SettingsPage>();
        services.AddSingleton<MainWindow>();

        return services.BuildServiceProvider();
    }

    public static T GetService<T>() where T : class =>
        (Current as App)!._serviceProvider.GetRequiredService<T>();
}
