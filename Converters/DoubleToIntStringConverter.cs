using Microsoft.UI.Xaml.Data;

namespace PaletteStudio.Converters;

public sealed class DoubleToIntStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is double d ? ((int)Math.Round(d)).ToString() : "0";

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}
