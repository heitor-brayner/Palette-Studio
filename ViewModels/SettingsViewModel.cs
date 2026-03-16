using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using PaletteStudio.Contracts;
using System.Reflection;

namespace PaletteStudio.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly IThemeService _themeService;

    [ObservableProperty]
    private int _themeIndex; // 0=System, 1=Light, 2=Dark

    public string AppVersion { get; } =
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

    public string GitHubUrl => "https://github.com/heitor-brayner/Palette-Studio";

    public SettingsViewModel(IThemeService themeService)
    {
        _themeService = themeService;
        _themeIndex = themeService.CurrentTheme switch
        {
            ElementTheme.Default => 0,
            ElementTheme.Light   => 1,
            ElementTheme.Dark    => 2,
            _                    => 0,
        };
    }

    partial void OnThemeIndexChanged(int value)
    {
        var theme = value switch
        {
            1 => ElementTheme.Light,
            2 => ElementTheme.Dark,
            _ => ElementTheme.Default,
        };
        _ = _themeService.SetThemeAsync(theme);
    }

    [RelayCommand]
    private async Task OpenGitHubAsync()
    {
        await Windows.System.Launcher.LaunchUriAsync(new Uri(GitHubUrl));
    }
}
