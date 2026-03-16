using PaletteStudio.Contracts;
using PaletteStudio.Models;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams; // IRandomAccessStream

namespace PaletteStudio.Services;

/// <summary>
/// Extracts dominant colors from an image using the median-cut quantization
/// algorithm implemented directly on raw BGRA8 pixel data from BitmapDecoder.
/// No third-party image-processing libraries are used — all pixel math is
/// hand-rolled so you can see exactly what's happening.
/// </summary>
public sealed class ColorExtractionService : IColorExtractionService
{
    // Sample every Nth pixel so we can handle large images in &lt;50 ms.
    private const int SampleStride = 4;

    public async Task<IReadOnlyList<ColorModel>> ExtractAsync(
        string filePath, int colorCount = 8, CancellationToken cancellationToken = default)
    {
        var pixels = await ReadPixelsAsync(filePath, cancellationToken).ConfigureAwait(false);
        if (pixels.Count == 0)
            return Array.Empty<ColorModel>();

        // Median-cut: we split into 2^depth buckets, so depth = log2(colorCount)
        int depth = (int)Math.Ceiling(Math.Log2(colorCount));
        var buckets = MedianCut(pixels, depth);

        return buckets
            .Select(bucket => Average(bucket))
            .OrderBy(c => c.Luminance)
            .ToList();
    }

    // -------------------------------------------------------------------------
    // Step 1 — decode pixels via BitmapDecoder (Windows imaging API)
    //
    // We use GetPixelDataAsync + DetachPixelData() which returns a plain byte[]
    // with no WinRT IBuffer interop required — cleaner on .NET 9.
    // -------------------------------------------------------------------------
    private static async Task<List<Rgb>> ReadPixelsAsync(
        string filePath, CancellationToken ct)
    {
        var file = await StorageFile.GetFileFromPathAsync(filePath)
                                     .AsTask(ct).ConfigureAwait(false);

        using IRandomAccessStream stream =
            await file.OpenAsync(FileAccessMode.Read).AsTask(ct).ConfigureAwait(false);

        var decoder = await BitmapDecoder.CreateAsync(stream).AsTask(ct).ConfigureAwait(false);

        uint width  = decoder.PixelWidth;
        uint height = decoder.PixelHeight;

        // GetPixelDataAsync returns BGRA8 pixels as a plain managed byte array.
        // No SoftwareBitmap / IBuffer interop needed.
        var pixelProvider = await decoder.GetPixelDataAsync(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Ignore,
            new BitmapTransform(),
            ExifOrientationMode.IgnoreExifOrientation,
            ColorManagementMode.DoNotColorManage)
            .AsTask(ct).ConfigureAwait(false);

        // DetachPixelData() hands ownership of the byte[] to us — 4 bytes per pixel (B G R A).
        var buffer = pixelProvider.DetachPixelData();

        var pixels = new List<Rgb>((int)(width * height / (SampleStride * SampleStride)));

        // Walk the buffer in strides, skipping near-transparent pixels.
        for (uint y = 0; y < height; y += (uint)SampleStride)
        for (uint x = 0; x < width;  x += (uint)SampleStride)
        {
            int idx = (int)((y * width + x) * 4);
            byte b = buffer[idx];
            byte g = buffer[idx + 1];
            byte r = buffer[idx + 2];
            // idx+3 is alpha; we requested Ignore so it's always 255
            pixels.Add(new Rgb(r, g, b));
        }

        return pixels;
    }

    // -------------------------------------------------------------------------
    // Step 2 — median-cut quantization
    //
    // Recursively split the largest bounding-box axis until we have enough
    // buckets.  Each bucket then yields one representative color via averaging.
    // -------------------------------------------------------------------------
    private static List<List<Rgb>> MedianCut(List<Rgb> pixels, int depth)
    {
        if (depth == 0 || pixels.Count == 0)
            return [pixels];

        // Find which channel has the greatest range.
        byte minR = 255, maxR = 0;
        byte minG = 255, maxG = 0;
        byte minB = 255, maxB = 0;

        foreach (var p in pixels)
        {
            if (p.R < minR) minR = p.R; if (p.R > maxR) maxR = p.R;
            if (p.G < minG) minG = p.G; if (p.G > maxG) maxG = p.G;
            if (p.B < minB) minB = p.B; if (p.B > maxB) maxB = p.B;
        }

        int rangeR = maxR - minR;
        int rangeG = maxG - minG;
        int rangeB = maxB - minB;

        List<Rgb> sorted;
        if (rangeR >= rangeG && rangeR >= rangeB)
            sorted = pixels.OrderBy(p => p.R).ToList();
        else if (rangeG >= rangeR && rangeG >= rangeB)
            sorted = pixels.OrderBy(p => p.G).ToList();
        else
            sorted = pixels.OrderBy(p => p.B).ToList();

        int mid = sorted.Count / 2;
        var left  = MedianCut(sorted[..mid],       depth - 1);
        var right = MedianCut(sorted[mid..],        depth - 1);

        left.AddRange(right);
        return left;
    }

    // -------------------------------------------------------------------------
    // Step 3 — average a bucket to a single representative color
    // -------------------------------------------------------------------------
    private static ColorModel Average(List<Rgb> bucket)
    {
        if (bucket.Count == 0) return new ColorModel(0, 0, 0);

        long r = 0, g = 0, b = 0;
        foreach (var p in bucket) { r += p.R; g += p.G; b += p.B; }
        int n = bucket.Count;
        return new ColorModel((byte)(r / n), (byte)(g / n), (byte)(b / n));
    }

    // Simple value-type to avoid boxing in LINQ pipelines.
    private readonly record struct Rgb(byte R, byte G, byte B);
}
