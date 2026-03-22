using System.Globalization;

namespace MauiApp1.Converters;

public class CategoryToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var category = value?.ToString() ?? string.Empty;

        return category switch
        {
            "Food" => Color.FromArgb("#FF6B6B"),
            "Travel" => Color.FromArgb("#4D96FF"),
            "Bills" => Color.FromArgb("#F7B267"),
            "Entertainment" => Color.FromArgb("#8E7DFF"),
            _ => Colors.Gray
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
