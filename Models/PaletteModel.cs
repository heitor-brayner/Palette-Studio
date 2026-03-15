namespace PaletteStudio.Models;

/// <summary>Immutable snapshot of an extracted palette and its source.</summary>
public sealed record PaletteModel(
    string SourcePath,
    string SourceName,
    IReadOnlyList<ColorModel> Colors,
    DateTimeOffset ExtractedAt)
{
    public static PaletteModel FromColors(string sourcePath, IReadOnlyList<ColorModel> colors) =>
        new(sourcePath, Path.GetFileName(sourcePath), colors, DateTimeOffset.Now);
}
