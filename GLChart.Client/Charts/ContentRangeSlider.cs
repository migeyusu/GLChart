using System.Windows;
using MahApps.Metro.Controls;

namespace GLChart.Samples.Charts;

public class ContentRangeSlider : RangeSlider
{
    static ContentRangeSlider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ContentRangeSlider), new FrameworkPropertyMetadata(typeof(ContentRangeSlider)));
    }

    public FrameworkElement Content
    {
        get { return (FrameworkElement)GetValue(ContentProperty); }
        set { SetValue(ContentProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register("Content", typeof(FrameworkElement), typeof(ContentRangeSlider), new PropertyMetadata(null));
        
}
