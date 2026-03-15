using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace PaletteStudio.Models;

/// <summary>Immutable representation of a single extracted color.</summary>
public sealed record ColorModel(byte R, byte G, byte B)
{
    public string Hex => $"#{R:X2}{G:X2}{B:X2}";

    public double Luminance => 0.2126 * Linearize(R / 255.0)
                             + 0.7152 * Linearize(G / 255.0)
                             + 0.0722 * Linearize(B / 255.0);

    public SolidColorBrush Brush => new(Color.FromArgb(255, R, G, B));

    public Color ToColor() => Color.FromArgb(255, R, G, B);

    // HSL components (0-360, 0-1, 0-1)
    public (double H, double S, double L) ToHsl()
    {
        double r = R / 255.0, g = G / 255.0, b = B / 255.0;
        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double l = (max + min) / 2.0;

        if (max == min) return (0, 0, l);

        double d = max - min;
        double s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);
        double h;
        if (max == r)      h = (g - b) / d + (g < b ? 6 : 0);
        else if (max == g) h = (b - r) / d + 2;
        else               h = (r - g) / d + 4;

        return (h * 60, s, l);
    }

    public static ColorModel FromHsl(double h, double s, double l)
    {
        if (s == 0)
        {
            byte gray = (byte)(l * 255);
            return new ColorModel(gray, gray, gray);
        }

        double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
        double p = 2 * l - q;
        double hNorm = h / 360.0;

        return new ColorModel(
            (byte)(HueToRgb(p, q, hNorm + 1.0 / 3.0) * 255),
            (byte)(HueToRgb(p, q, hNorm) * 255),
            (byte)(HueToRgb(p, q, hNorm - 1.0 / 3.0) * 255));
    }

    private static double HueToRgb(double p, double q, double t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1.0 / 6.0) return p + (q - p) * 6 * t;
        if (t < 1.0 / 2.0) return q;
        if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;
        return p;
    }

    private static double Linearize(double c) =>
        c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
}
