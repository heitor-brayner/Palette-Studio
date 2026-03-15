using Microsoft.UI.Xaml.Controls;

namespace PaletteStudio.Contracts;

public interface INavigationService
{
    bool CanGoBack { get; }
    event EventHandler<string>? Navigated;

    void SetFrame(Frame frame);
    bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false);
    bool GoBack();
}
