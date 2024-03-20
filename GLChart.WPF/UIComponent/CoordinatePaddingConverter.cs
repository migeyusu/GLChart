using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GLChart.WPF.UIComponent;

public class CoordinatePaddingConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var orientation = (Orientation)(parameter ?? Orientation.Horizontal);
        if (value is Thickness thickness)
        {
            if (orientation == Orientation.Horizontal)
            {
                return new Thickness(thickness.Left, 0, thickness.Right, 0);
            }
            else
            {
                return new Thickness(0, thickness.Top, 0, thickness.Bottom);
            }
        }

        return default;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}