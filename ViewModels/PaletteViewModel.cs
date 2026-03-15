using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using PaletteStudio.Contracts;
using PaletteStudio.Models;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace PaletteStudio.ViewModels;

public sealed partial class PaletteViewModel : ObservableObject
{
    private readonly IColorExtractionService _extractionService;
    private readonly INavigationService _navigationService;
    private readonly DispatcherQueue _dispatcher;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _sourceImagePath;

    [ObservableProperty]
    private string? _sourceName;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private ColorModel? _selectedColor;

    public ObservableCollection<ColorModel> Colors { get; } = [];

    public PaletteViewModel(
        IColorExtractionService extractionService,
        INavigationService navigationService)
    {
        _extractionService = extractionService;
        _navigationService = navigationService;
        _dispatcher = DispatcherQueue.GetForCurrentThread();
    }

    [RelayCommand]
    private async Task BrowseImageAsync()
    {
        var picker = new FileOpenPicker();

        // REASON: FileOpenPicker requires a window handle in unpackaged WinUI 3 apps.
        var hWnd = WindowNative.GetWindowHandle(App.MainWindow!);
        InitializeWithWindow.Initialize(picker, hWnd);

        picker.ViewMode = PickerViewMode.Thumbnail;
        picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".webp");
        picker.FileTypeFilter.Add(".bmp");

        var file = await picker.PickSingleFileAsync();
        if (file is not null)
            await ExtractFromPathAsync(file.Path);
    }

    public Task LoadFromPathAsync(string path) => ExtractFromPathAsync(path);

    private async Task ExtractFromPathAsync(string path)
    {
        ErrorMessage = null;
        IsLoading = true;
        Colors.Clear();

        try
        {
            var colors = await _extractionService.ExtractAsync(path).ConfigureAwait(false);

            _dispatcher.TryEnqueue(() =>
            {
                SourceImagePath = path;
                SourceName = Path.GetFileName(path);
                foreach (var c in colors)
                    Colors.Add(c);
            });
        }
        catch (Exception ex)
        {
            _dispatcher.TryEnqueue(() => ErrorMessage = $"Could not process image: {ex.Message}");
        }
        finally
        {
            _dispatcher.TryEnqueue(() => IsLoading = false);
        }
    }

    [RelayCommand]
    private void SelectColor(ColorModel color)
    {
        SelectedColor = color;
        _navigationService.NavigateTo(nameof(Views.ColorDetailPage), color);
    }

    [RelayCommand]
    private void RemoveColor(ColorModel color)
    {
        Colors.Remove(color);
    }
}
