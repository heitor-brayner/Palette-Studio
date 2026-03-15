using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using PaletteStudio.Models;
using PaletteStudio.ViewModels;
using Windows.ApplicationModel.DataTransfer;

namespace PaletteStudio.Views;

public sealed partial class PalettePage : Page
{
    public PaletteViewModel ViewModel { get; }

    public PalettePage()
    {
        ViewModel = App.GetService<PaletteViewModel>();
        InitializeComponent();

        SwatchRepeater.ElementPrepared += OnSwatchPrepared;
    }

    // ── Staggered entrance animation ──────────────────────────────────────
    // Each swatch slides up 24 px and fades in, delayed by index * 50 ms.
    private void OnSwatchPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
    {
        var element   = args.Element;
        var visual    = ElementCompositionPreview.GetElementVisual(element);
        var compositor = visual.Compositor;

        visual.Opacity = 0f;
        visual.Offset  = new System.Numerics.Vector3(0, 24, 0);

        var delay = TimeSpan.FromMilliseconds(args.Index * 50);

        var fadeIn = compositor.CreateScalarKeyFrameAnimation();
        fadeIn.Target    = "Opacity";
        fadeIn.DelayTime = delay;
        fadeIn.InsertKeyFrame(0f, 0f);
        fadeIn.InsertKeyFrame(1f, 1f);
        fadeIn.Duration  = TimeSpan.FromMilliseconds(300);

        var slideUp = compositor.CreateVector3KeyFrameAnimation();
        slideUp.Target    = "Offset";
        slideUp.DelayTime = delay;
        slideUp.InsertKeyFrame(0f, new System.Numerics.Vector3(0, 24, 0));
        slideUp.InsertKeyFrame(1f, new System.Numerics.Vector3(0, 0, 0));
        slideUp.Duration  = TimeSpan.FromMilliseconds(300);

        visual.StartAnimation("Opacity", fadeIn);
        visual.StartAnimation("Offset",  slideUp);
    }

    // ── Drag & drop ──────────────────────────────────────────────────────

    private void DropZone_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
        if (e.DragUIOverride is { } ui)
        {
            ui.Caption = "Drop to extract palette";
            ui.IsGlyphVisible = true;
        }
        DropZoneBorder.Opacity = 0.7;
    }

    private void DropZone_DragLeave(object sender, DragEventArgs e)
    {
        DropZoneBorder.Opacity = 1.0;
    }

    private async void DropZone_Drop(object sender, DragEventArgs e)
    {
        DropZoneBorder.Opacity = 1.0;

        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            var file  = items.OfType<Windows.Storage.StorageFile>().FirstOrDefault();
            if (file is not null)
                await ViewModel.LoadFromPathAsync(file.Path);
        }
    }

    // ── Swatch tap → ConnectedAnimation → ColorDetailPage ───────────────

    private void SwatchCard_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is not FrameworkElement el) return;
        if (el.DataContext is not ColorModel color) return;

        // Find the color preview block inside the swatch card for the animation.
        var colorBlock = el.FindName("ColorPreviewBlock") as UIElement ?? el;
        ConnectedAnimationService.GetForCurrentView()
            .PrepareToAnimate("SwatchToDetail", colorBlock);

        ViewModel.SelectColorCommand.Execute(color);
    }

    // ── Copy hex to clipboard ────────────────────────────────────────────

    private void CopyHex_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement el && el.DataContext is ColorModel color)
        {
            var dp = new DataPackage();
            dp.SetText(color.Hex);
            Clipboard.SetContent(dp);
        }
    }
}
