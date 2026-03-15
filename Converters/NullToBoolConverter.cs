using Microsoft.UI.Xaml.Data;

namespace PaletteStudio.Converters;

/// <summary>null/empty string → false, any value → true (used for InfoBar.IsOpen).</summary>
public sealed class NullToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is string s && !string.IsNullOrEmpty(s);

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}
