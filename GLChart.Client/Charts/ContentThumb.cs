using System.Windows;
using MahApps.Metro.Controls;

namespace GLChart.Samples.Charts;

public class ContentThumb : MetroThumb
{
    static ContentThumb()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ContentThumb),
            new FrameworkPropertyMetadata(typeof(ContentThumb)));
    }

    public FrameworkElement Content {
        get { return (FrameworkElement) GetValue(ContentProperty); }
        set { SetValue(ContentProperty, value); }
    }

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register("Content", typeof(FrameworkElement), typeof(ContentThumb),
            new PropertyMetadata(null));
        
}