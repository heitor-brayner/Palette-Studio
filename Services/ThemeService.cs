using Microsoft.UI.Xaml;
using PaletteStudio.Contracts;
using Windows.Storage;

namespace PaletteStudio.Services;

public sealed class ThemeService : IThemeService
{
    private const string ThemeKey = "AppTheme";
    private FrameworkElement? _root;

    public ElementTheme CurrentTheme { get; private set; } = ElementTheme.Default;

    public void Initialize(FrameworkElement root)
    {
        _root = root;
        var saved = ReadSavedTheme();
        ApplyTheme(saved);
    }

    public Task SetThemeAsync(ElementTheme theme)
    {
        ApplyTheme(theme);
        SaveTheme(theme);
        return Task.CompletedTask;
    }

    private void ApplyTheme(ElementTheme theme)
    {
        CurrentTheme = theme;
        if (_root is not null)
            _root.RequestedTheme = theme;
    }

    private static ElementTheme ReadSavedTheme()
    {
        try
        {
            // REASON: ApplicationData.Current is available for unpackaged apps
            // via the Windows App SDK bootstrap; safe to call after App.OnLaunched.
            var settings = ApplicationData.Current.LocalSettings;
            if (settings.Values[ThemeKey] is string raw &&
                Enum.TryParse<ElementTheme>(raw, out var theme))
            {
                return theme;
            }
        }
        catch { /* first run or restricted environment */ }

        return ElementTheme.Default;
    }

    private static void SaveTheme(ElementTheme theme)
    {
        try
        {
            ApplicationData.Current.LocalSettings.Values[ThemeKey] = theme.ToString();
        }
        catch { /* best-effort */ }
    }
}
