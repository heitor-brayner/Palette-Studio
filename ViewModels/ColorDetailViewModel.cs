using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using PaletteStudio.Contracts;
using PaletteStudio.Models;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;

namespace PaletteStudio.ViewModels;

public sealed partial class ColorDetailViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private bool _isSyncing; // guard against recursive property updates

    // ── Raw channel properties (bound to sliders) ──────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewBrush))]
    [NotifyPropertyChangedFor(nameof(HexText))]
    private double _r;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewBrush))]
    [NotifyPropertyChangedFor(nameof(HexText))]
    private double _g;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewBrush))]
    [NotifyPropertyChangedFor(nameof(HexText))]
    private double _b;

    // ── HSL properties ──────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewBrush))]
    [NotifyPropertyChangedFor(nameof(HexText))]
    private double _hue;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewBrush))]
    [NotifyPropertyChangedFor(nameof(HexText))]
    private double _saturation;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewBrush))]
    [NotifyPropertyChangedFor(nameof(HexText))]
    private double _lightness;

    // ── Hex field ───────────────────────────────────────────────────────────

    [ObservableProperty]
    private string _hexText = "#000000";

    // ── Computed ────────────────────────────────────────────────────────────

    public SolidColorBrush PreviewBrush =>
        new(Color.FromArgb(255, (byte)R, (byte)G, (byte)B));

    public ColorDetailViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public void LoadColor(ColorModel color)
    {
        _isSyncing = true;
        R = color.R; G = color.G; B = color.B;
        var (h, s, l) = color.ToHsl();
        Hue = h; Saturation = s * 100; Lightness = l * 100;
        HexText = color.Hex;
        _isSyncing = false;
    }

    // Called when RGB sliders change → sync HSL + hex
    partial void OnRChanged(double value) => SyncFromRgb();
    partial void OnGChanged(double value) => SyncFromRgb();
    partial void OnBChanged(double value) => SyncFromRgb();

    private void SyncFromRgb()
    {
        if (_isSyncing) return;
        _isSyncing = true;
        var model = new ColorModel((byte)R, (byte)G, (byte)B);
        var (h, s, l) = model.ToHsl();
        Hue = h; Saturation = s * 100; Lightness = l * 100;
        HexText = model.Hex;
        OnPropertyChanged(nameof(PreviewBrush));
        _isSyncing = false;
    }

    // Called when HSL sliders change → sync RGB + hex
    partial void OnHueChanged(double value) => SyncFromHsl();
    partial void OnSaturationChanged(double value) => SyncFromHsl();
    partial void OnLightnessChanged(double value) => SyncFromHsl();

    private void SyncFromHsl()
    {
        if (_isSyncing) return;
        _isSyncing = true;
        var model = ColorModel.FromHsl(Hue, Saturation / 100.0, Lightness / 100.0);
        R = model.R; G = model.G; B = model.B;
        HexText = model.Hex;
        OnPropertyChanged(nameof(PreviewBrush));
        _isSyncing = false;
    }

    public ColorModel ToColorModel() => new((byte)R, (byte)G, (byte)B);

    [RelayCommand]
    private void CopyHex()
    {
        var dp = new DataPackage();
        dp.SetText(HexText);
        Clipboard.SetContent(dp);
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();
}
