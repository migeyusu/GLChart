using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;

namespace GLChart.WPF.Base;

public class ScrollRangeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        if (destinationType == typeof(string))
        {
            return true;
        }

        return base.CanConvertTo(context, destinationType);
    }


    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value == null)
        {
            throw GetConvertFromException(value);
        }

        if (value is string source)
        {
            var point = Point.Parse(source);
            return new ScrollRange(point.X, point.Y);
        }

        return base.ConvertFrom(context, culture, value);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value,
        Type destinationType)
    {
        if (value is Point)
        {
            ScrollRange instance = (ScrollRange)value;
            if (destinationType == typeof(string))
            {
                // Delegate to the formatting/culture-aware ConvertToString method.

#pragma warning suppress 6506 // instance is obviously not null
                return instance.ToString();
            }
        }

        // Pass unhandled cases to base class (which will throw exceptions for null value or destinationType.)
        return base.ConvertTo(context, culture, value, destinationType);
    }
}