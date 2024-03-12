using System;
using System.Globalization;
using System.Windows.Data;
using GLChart.WPF.Base;

namespace GLChart.Samples.Charts;

public class ScrollRangeToScalarConverter : IValueConverter
{
    public bool IsUpperValue { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ScrollRange scrollRange)
        {
            return IsUpperValue ? scrollRange.End : scrollRange.Start;
        }

        return 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}