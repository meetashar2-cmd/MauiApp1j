using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace MauiApp1.Converters;

public class CategoryToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var category = value?.ToString()?.ToLower();

        return category switch
        {
            "food" => Colors.Orange,
            "shopping" => Colors.MediumPurple,
            "other" => Colors.Gray,
            _ => Colors.Blue
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}