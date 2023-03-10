using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using OpenTK;
using OpenTK.Graphics;
using OpenTkWPFHost.Configuration;
using RLP.Chart.Interface.Abstraction;
using RLP.Chart.OpenGL.Renderer;
using Color = System.Drawing.Color;

namespace RLP.Chart.Client
{
    /// <summary>
    /// TestWindow.xaml 的交互逻辑
    /// </summary>
    public partial class RenderTestWindow : Window
    {
        public RenderTestWindow()
        {
            InitializeChannel();
            InitializeComponent();
            ThreadOpenTkControl.GlSettings = new GLSettings()
            {
                GraphicsContextFlags = GraphicsContextFlags.Offscreen,
                GraphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8, 4)
            };
            ThreadOpenTkControl.OpenGlErrorReceived += ThreadOpenTkControl_OpenGlErrorReceived;
            ThreadOpenTkControl.RenderErrorReceived += ThreadOpenTkControl_RenderErrorReceived;
            ThreadOpenTkControl.Renderer = _coordinate3DRenderer;
        }

        private ChannelSeriesRenderer channelSeriesRenderer;
        private ChannelRenderer _channelRenderer;
        private int _totalChannelCount = 0;
        private const int ChannelWidth = 30;
        private const float XInterval = 100;
        private const float YInterval = 100;
        private const float ZStart = 500;
        private float _currentZ = ZStart;
        private float _zMax = ZStart;
        private const int MaxChannelCount = 50;
        private Matrix4 rotationY = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(270.0f));
        private Matrix4 rotationX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(270.0f));

        private void InitializeChannel()
        {
            var firstChannelPoints = new IPoint3D[ChannelWidth];
            for (int i = 0; i < ChannelWidth; i++)
            {
                firstChannelPoints[i] = new Point3D(0, i * YInterval, 0);
            }

            _totalChannelCount++;
            _channelRenderer = new ChannelRenderer()
            {
                ChannelColor = Color.Red,
                ChannelWidth = ChannelWidth,
                MaxChannelCount = MaxChannelCount,
            };
            _channelRenderer.Add(new Channel(firstChannelPoints));
            channelSeriesRenderer = new ChannelSeriesRenderer(new Shader("Shaders/ChannelShader/shader.vert",
                "Shaders/ChannelShader/shader.frag"))
            {
            };
            channelSeriesRenderer.Add(_channelRenderer);
            _coordinate3DRenderer = new Coordinate3DRenderer(new List<BaseRenderer>() { channelSeriesRenderer })
            {
                BackgroundColor = Color4.DodgerBlue,
                View = rotationY * rotationX,
            };
            AddChannels(5);
        }

        private void ThreadOpenTkControl_RenderErrorReceived(object sender, OpenTkWPFHost.Core.RenderErrorArgs e)
        {
            Debug.WriteLine($"{e.Phase}:{e.Exception}");
        }

        private void ThreadOpenTkControl_OpenGlErrorReceived(object sender, OpenTkWPFHost.Core.OpenGlErrorArgs e)
        {
            Debug.WriteLine(e.ErrorMessage);
        }


        private Coordinate3DRenderer _coordinate3DRenderer;

        private void RenderTestWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void XRotationRangeBase_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var eNewValue = (float)e.NewValue;
            rotationX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(eNewValue));
            _coordinate3DRenderer.View = rotationX * rotationY;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        private void AddChannelsButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            AddChannels(5);
        }

        private void AddChannels(int appendChannelCount)
        {
            var random = new Random();
            var channels = new IChannel[appendChannelCount];
            for (int i = 0; i < appendChannelCount; i++)
            {
                _currentZ += random.Next(-20, 20);
                var lineZ = _currentZ;
                var points = new IPoint3D[ChannelWidth];
                var channelStart = (_totalChannelCount + i) * YInterval;
                for (int j = 0; j < ChannelWidth; j++)
                {
                    if (_zMax < lineZ)
                    {
                        _zMax = lineZ;
                    }

                    points[j] = new Point3D(channelStart, j * XInterval, lineZ);
                    lineZ += random.Next(-50, 50);
                }

                channels[i] = new Channel(points);
            }

            _channelRenderer.AddRange(channels);
            _totalChannelCount += appendChannelCount;
            channelSeriesRenderer.ZHighest = _zMax;
            var leftXInterval = _totalChannelCount < MaxChannelCount
                ? -XInterval
                : (_totalChannelCount - MaxChannelCount - 1) * XInterval;
            _coordinate3DRenderer.Projection = Matrix4.CreateOrthographicOffCenter(leftXInterval,
                _totalChannelCount * XInterval + XInterval,
                -YInterval, ChannelWidth * YInterval + YInterval, _zMax * 2, -_zMax * 2);
        }

        private void YRotationSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var eNewValue = (float)e.NewValue;
            rotationY = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(eNewValue));
            _coordinate3DRenderer.View = rotationX * rotationY;
        }
    }
}