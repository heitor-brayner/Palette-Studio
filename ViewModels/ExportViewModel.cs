using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PaletteStudio.Models;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace PaletteStudio.ViewModels;

public sealed partial class ExportViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewText))]
    private int _selectedFormatIndex;

    [ObservableProperty]
    private bool _isSaving;

    public ObservableCollection<ColorModel> Colors { get; } = [];

    public string PreviewText => SelectedFormatIndex switch
    {
        0 => BuildCss(),
        1 => BuildXaml(),
        2 => BuildJson(),
        _ => string.Empty,
    };

    public void LoadColors(IEnumerable<ColorModel> colors)
    {
        Colors.Clear();
        foreach (var c in colors)
            Colors.Add(c);
        OnPropertyChanged(nameof(PreviewText));
    }

    // ── Format builders ─────────────────────────────────────────────────────

    private string BuildCss()
    {
        var sb = new StringBuilder();
        sb.AppendLine(":root {");
        for (int i = 0; i < Colors.Count; i++)
            sb.AppendLine($"  --color-{i + 1}: {Colors[i].Hex};");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private string BuildXaml()
    {
        var sb = new StringBuilder();
        sb.AppendLine("<ResourceDictionary");
        sb.AppendLine("    xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
        sb.AppendLine("    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">");
        sb.AppendLine();
        for (int i = 0; i < Colors.Count; i++)
        {
            var c = Colors[i];
            sb.AppendLine($"    <Color x:Key=\"Color{i + 1}\">{c.Hex}FF</Color>");
            sb.AppendLine($"    <SolidColorBrush x:Key=\"Color{i + 1}Brush\" Color=\"{{StaticResource Color{i + 1}}}\"/>");
        }
        sb.AppendLine();
        sb.AppendLine("</ResourceDictionary>");
        return sb.ToString();
    }

    private string BuildJson()
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine("  \"palette\": [");
        for (int i = 0; i < Colors.Count; i++)
        {
            var c = Colors[i];
            bool last = i == Colors.Count - 1;
            sb.AppendLine($"    {{ \"index\": {i + 1}, \"hex\": \"{c.Hex}\", \"r\": {c.R}, \"g\": {c.G}, \"b\": {c.B} }}{(last ? "" : ",")}");
        }
        sb.AppendLine("  ]");
        sb.AppendLine("}");
        return sb.ToString();
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private void CopyToClipboard()
    {
        var dp = new DataPackage();
        dp.SetText(PreviewText);
        Clipboard.SetContent(dp);
    }

    [RelayCommand]
    private async Task SaveToFileAsync()
    {
        IsSaving = true;
        try
        {
            var (extension, suggestedName) = SelectedFormatIndex switch
            {
                0 => (".css",  "palette.css"),
                1 => (".xaml", "palette.xaml"),
                _  => (".json","palette.json"),
            };

            var picker = new FileSavePicker();
            var hWnd = WindowNative.GetWindowHandle(App.MainWindow!);
            InitializeWithWindow.Initialize(picker, hWnd);

            picker.SuggestedFileName = suggestedName;
            picker.FileTypeChoices.Add("Export file", [extension]);

            var file = await picker.PickSaveFileAsync();
            if (file is not null)
                await FileIO.WriteTextAsync(file, PreviewText);
        }
        finally
        {
            IsSaving = false;
        }
    }
}
