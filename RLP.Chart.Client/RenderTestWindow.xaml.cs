using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            InitializeComponent();
            ThreadOpenTkControl.GlSettings = new GLSettings()
            {
                GraphicsContextFlags = GraphicsContextFlags.Offscreen,
                GraphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8, 4)
            };
            ThreadOpenTkControl.OpenGlErrorReceived += ThreadOpenTkControl_OpenGlErrorReceived;
            ThreadOpenTkControl.RenderErrorReceived += ThreadOpenTkControl_RenderErrorReceived;
        }

        private void ThreadOpenTkControl_RenderErrorReceived(object sender, OpenTkWPFHost.Core.RenderErrorArgs e)
        {
            Debug.WriteLine($"{e.Phase}:{e.Exception.ToString()}");
        }

        private void ThreadOpenTkControl_OpenGlErrorReceived(object sender, OpenTkWPFHost.Core.OpenGlErrorArgs e)
        {
            Debug.WriteLine(e.ErrorMessage);
        }

        private Coordinate3DRenderer _coordinate3DRenderer;

        private void RenderTestWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            const int channelCount = 2;
            const int channelWidth = 2;
            const float xInterval = 100;
            const float yInterval = 100;
            float zStart = 100;
            float zMax = zStart;
            var random = new Random();
            var channels = new IChannel[channelCount];
            for (int i = 0; i < channelCount; i++)
            {
                zStart += random.Next(0, 10);
                var lineZ = zStart;
                var points = new IPoint3D[channelWidth];
                var interval = i * yInterval;
                for (int j = 0; j < channelWidth; j++)
                {
                    points[j] = new Point3D(interval, j * xInterval, lineZ);
                    lineZ += random.Next(0, 10);
                    if (zMax < lineZ)
                    {
                        zMax = lineZ;
                    }
                }

                channels[i] = new Channel() { Points = points };
            }

            var channelRenderer = new ChannelRenderer()
            {
                ChannelColor = Color.Red,
                ChannelWidth = channelWidth,
                MaxChannelCount = 1000,
            };
            channelRenderer.AddGeometries(channels);
            var channelSeriesRenderer = new ChannelSeriesRenderer(new Shader("Shaders/ChannelShader/shader.vert",
                "Shaders/ChannelShader/shader.frag")) { channelRenderer };
            var position = Matrix4.Identity; //CreateRotationX(MathHelper.DegreesToRadians(-55.0f));
            /*Matrix4 view = Matrix4.CreateTranslation(0.0f, 0.0f, -200.0f);
            Matrix4 projection = Matrix4.CreateOrthographicOffCenter(-50f, 150f, -50f, 150f, 0f, 200f);*/
            //Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f),(float)(800f / 450f), 1f, (float)300f);
            _coordinate3DRenderer = new Coordinate3DRenderer(new List<BaseRenderer>() { channelSeriesRenderer })
            {
                BackgroundColor = Color4.DodgerBlue,
                View = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-55.0f)),
                Projection = Matrix4.CreateOrthographicOffCenter(-1, 1, -1, 1, -100, 100),
            };
            ThreadOpenTkControl.Renderer = _coordinate3DRenderer;
        }

        private void RangeBase_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var eNewValue = (float)e.NewValue;
            _coordinate3DRenderer.View = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(eNewValue));
            TextBlock.Text = $"{eNewValue}°";
        }
    }
}