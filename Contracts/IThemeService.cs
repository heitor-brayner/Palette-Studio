using Microsoft.UI.Xaml;

namespace PaletteStudio.Contracts;

public interface IThemeService
{
    ElementTheme CurrentTheme { get; }
    void Initialize(FrameworkElement root);
    Task SetThemeAsync(ElementTheme theme);
}
