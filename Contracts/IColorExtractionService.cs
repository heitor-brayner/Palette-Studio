using PaletteStudio.Models;

namespace PaletteStudio.Contracts;

public interface IColorExtractionService
{
    /// <summary>
    /// Extracts up to <paramref name="colorCount"/> dominant colors from the image
    /// at <paramref name="filePath"/> using median-cut quantization.
    /// Returns colors sorted by luminance (dark → light).
    /// </summary>
    Task<IReadOnlyList<ColorModel>> ExtractAsync(string filePath, int colorCount = 8,
        CancellationToken cancellationToken = default);
}
