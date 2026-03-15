using Microsoft.UI.Xaml.Data;

namespace PaletteStudio.Converters;

public sealed class CopyLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        $"Copy {value}";

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}
