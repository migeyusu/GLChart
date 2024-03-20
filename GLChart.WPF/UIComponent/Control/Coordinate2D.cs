using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using GLChart.WPF.Render;
using GLChart.WPF.UIComponent.Axis;

namespace GLChart.WPF.UIComponent.Control
{
    /// <summary>
    /// 基于wpf的2d坐标轴和网格绘制
    /// </summary>
    [TemplatePart(Name = XAxisName, Type = typeof(AxisXOption))]
    [TemplatePart(Name = YAxisName, Type = typeof(AxisYOption))]
    [TemplatePart(Name = SeparatorLayerName, Type = typeof(MeshLayer))]
    public class Coordinate2D : ContentControl
    {
        private const string XAxisName = "XAxis";

        private const string YAxisName = "YAxis";

        public const string SeparatorLayerName = "SeparatorLayer";

        public static readonly DependencyProperty AxisXOptionProperty = DependencyProperty.Register(
            "AxisXOption", typeof(AxisXOption), typeof(Coordinate2D),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));

        public AxisXOption AxisXOption
        {
            get { return (AxisXOption)GetValue(AxisXOptionProperty); }
            set { SetValue(AxisXOptionProperty, value); }
        }

        public static readonly DependencyProperty AxisYOptionProperty = DependencyProperty.Register(
            "AxisYOption", typeof(AxisYOption), typeof(Coordinate2D),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));

        public AxisYOption AxisYOption
        {
            get { return (AxisYOption)GetValue(AxisYOptionProperty); }
            set { SetValue(AxisYOptionProperty, value); }
        }

        static Coordinate2D()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Coordinate2D),
                new FrameworkPropertyMetadata(typeof(Coordinate2D)));
        }
        
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}