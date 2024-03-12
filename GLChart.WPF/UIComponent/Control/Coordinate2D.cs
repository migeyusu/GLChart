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
    [TemplatePart(Name = XAxisName, Type = typeof(XAxisElement))]
    [TemplatePart(Name = YAxisName, Type = typeof(YAxisElement))]
    [TemplatePart(Name = SeparatorLayerName, Type = typeof(SeparatorLayer))]
    public class Coordinate2D : ContentControl
    {
        private const string XAxisName = "XAxis";

        private const string YAxisName = "YAxis";

        public const string SeparatorLayerName = "SeparatorLayer";

        public static readonly DependencyProperty AxisXOptionProperty = DependencyProperty.Register(
            "AxisXOption", typeof(AxisOption), typeof(Coordinate2D),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));

        public AxisOption AxisXOption
        {
            get { return (AxisOption)GetValue(AxisXOptionProperty); }
            set { SetValue(AxisXOptionProperty, value); }
        }

        public static readonly DependencyProperty AxisYOptionProperty = DependencyProperty.Register(
            "AxisYOption", typeof(AxisOption), typeof(Coordinate2D),
            new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsRender));

        public AxisOption AxisYOption
        {
            get { return (AxisOption)GetValue(AxisYOptionProperty); }
            set { SetValue(AxisYOptionProperty, value); }
        }

        static Coordinate2D()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Coordinate2D),
                new FrameworkPropertyMetadata(typeof(Coordinate2D)));
        }

        protected SeparatorLayer SeparatorLayer;

        protected XAxisElement XAxisElement;

        protected YAxisElement YAxisElement;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            XAxisElement = GetTemplateChild(XAxisName) as XAxisElement;
            YAxisElement = GetTemplateChild(YAxisName) as YAxisElement;
            SeparatorLayer = GetTemplateChild(SeparatorLayerName) as SeparatorLayer;
        }
    }
}