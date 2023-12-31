﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using GLChart.Interface;

namespace GLChart.OpenTK.Control
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

        public static readonly DependencyProperty CoordinateRegionProperty = DependencyProperty.Register(
            "CoordinateRegion", typeof(Region2D), typeof(Coordinate2D),
            new FrameworkPropertyMetadata(default(Region2D)));

        public Region2D CoordinateRegion
        {
            get { return (Region2D)GetValue(CoordinateRegionProperty); }
            set { SetValue(CoordinateRegionProperty, value); }
        }

        public static readonly DependencyProperty XLabelGenerationOptionProperty = DependencyProperty.Register(
            "XLabelGenerationOption", typeof(AxisOption), typeof(Coordinate2D),
            new PropertyMetadata(new AxisOption()));

        public AxisOption XLabelGenerationOption
        {
            get { return (AxisOption)GetValue(XLabelGenerationOptionProperty); }
            set { SetValue(XLabelGenerationOptionProperty, value); }
        }

        public static readonly DependencyProperty YLabelGenerationOptionProperty = DependencyProperty.Register(
            "YLabelGenerationOption", typeof(AxisOption), typeof(Coordinate2D),
            new PropertyMetadata(new AxisOption()));

        public AxisOption YLabelGenerationOption
        {
            get { return (AxisOption)GetValue(YLabelGenerationOptionProperty); }
            set { SetValue(YLabelGenerationOptionProperty, value); }
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
            var coordinateRegion = CoordinateRegion;
            XAxisElement.Range = coordinateRegion.XRange;
            YAxisElement.Range = coordinateRegion.YRange;
            SeparatorLayer = GetTemplateChild(SeparatorLayerName) as SeparatorLayer;
            DependencyPropertyDescriptor.FromProperty(CoordinateRegionProperty, typeof(Coordinate2D))
                .AddValueChanged(this, CoordinateRegionChangedHandler);
        }

        private void CoordinateRegionChangedHandler(object sender, EventArgs e)
        {
            var coordinateRegion = this.CoordinateRegion;
            this.XAxisElement.Range = coordinateRegion.XRange;
            this.YAxisElement.Range = coordinateRegion.YRange;
        }
    }
}