using System;
using System.ComponentModel;
using System.Windows;
using GLChart.WPF.Base;
using MahApps.Metro.Controls;

namespace GLChart.Samples.Charts;

public class ContentRangeSlider : RangeSlider
{
    static ContentRangeSlider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ContentRangeSlider),
            new FrameworkPropertyMetadata(typeof(ContentRangeSlider)));
    }

    public ContentRangeSlider()
    {
        DependencyPropertyDescriptor.FromProperty(UpperValueProperty, typeof(RangeSlider))
            .AddValueChanged(this, new EventHandler(OnRangeChangedEventHandler));
        DependencyPropertyDescriptor.FromProperty(LowerValueProperty, typeof(RangeSlider))
            .AddValueChanged(this, new EventHandler(OnRangeChangedEventHandler));
    }

    private void OnRangeChangedEventHandler(object? sender, EventArgs e)
    {
        this.ActualRange = new ScrollRange(LowerValue, UpperValue);
    }

    public static readonly DependencyProperty ActualRangeProperty = DependencyProperty.Register(
        nameof(ActualRange), typeof(ScrollRange), typeof(ContentRangeSlider),
        new PropertyMetadata(default(ScrollRange), (OnActualRangeChangedCallback)));

    private static void OnActualRangeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ContentRangeSlider rangeSlider)
        {
            var newValue = (ScrollRange)e.NewValue;
            rangeSlider.UpperValue = newValue.End;
            rangeSlider.LowerValue = newValue.Start;
        }
    }

    public ScrollRange ActualRange
    {
        get { return (ScrollRange)GetValue(ActualRangeProperty); }
        set { SetValue(ActualRangeProperty, value); }
    }

    public static readonly DependencyProperty WholeRangeProperty = DependencyProperty.Register(
        nameof(WholeRange), typeof(ScrollRange), typeof(ContentRangeSlider),
        new PropertyMetadata(default(ScrollRange), OnBoundaryChangedCallback));

    private static void OnBoundaryChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ContentRangeSlider slider)
        {
            var newValue = (ScrollRange)e.NewValue;
            slider.Minimum = newValue.Start;
            slider.Maximum = newValue.End;
        }
    }

    public ScrollRange WholeRange
    {
        get { return (ScrollRange)GetValue(WholeRangeProperty); }
        set { SetValue(WholeRangeProperty, value); }
    }

    public FrameworkElement Content
    {
        get { return (FrameworkElement)GetValue(ContentProperty); }
        set { SetValue(ContentProperty, value); }
    }

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register("Content", typeof(FrameworkElement), typeof(ContentRangeSlider),
            new PropertyMetadata(null));
}