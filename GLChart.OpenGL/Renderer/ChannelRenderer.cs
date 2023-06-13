using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTkWPFHost.Core;
using RLP.Chart.Interface;
using RLP.Chart.Interface.Abstraction;
using RLP.Chart.OpenGL.Abstraction;

namespace RLP.Chart.OpenGL.Renderer
{
    /// <summary>
    /// single channel renderer
    /// </summary>
    public class ChannelRenderer : IShaderRendererItem, IGeometryCollection<IChannel>
    {
        private Color4 _color4 = Color4.Red;

        public Color ChannelColor
        {
            get => Color.FromArgb(_color4.ToArgb());
            set => _color4 = new Color4(value.R, value.G, value.B, value.A);
        }

        private const int SizeFloat = sizeof(float);

        private uint _channelWidth;

        /// <summary>
        /// 通道宽度，指通道点位数量
        /// </summary>
        public uint ChannelWidth
        {
            get => _channelWidth;
            set
            {
                if (value < 2)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _channelWidth = value;
                this.ChannelBufferWidth = value * 3;
            }
        }

        public uint MaxChannelCount { get; set; }

        protected int VertexBufferObject;

        protected int ElementBufferObject;

        protected int VertexArrayObject;

        public bool IsInitialized { get; private set; }

        public Guid Id { get; } = Guid.NewGuid();

        public bool RenderEnable { get; private set; } = true;

        private ModelRingBuffer<IChannel, float> _channelBuffer = new ModelRingBuffer<IChannel, float>();

        private Shader _shader;

        public ChannelRenderer()
        {
        }

        private const int SizeOfUint = sizeof(uint);

        /// <summary>
        /// 真实的浮点缓冲宽度
        /// </summary>
        public uint ChannelBufferWidth { get; set; }

        public void Initialize(IGraphicsContext context)
        {
            if (IsInitialized)
            {
                return;
            }

            var channelBufferLength = ChannelBufferWidth;
            _channelBuffer.Initialize(this.MaxChannelCount, channelBufferLength, channel =>
            {
                var buffer = new float[channelBufferLength];
                var index = 0;
                foreach (var point in channel.Points)
                {
                    buffer[index] = point.X;
                    buffer[index + 1] = point.Y;
                    buffer[index + 2] = point.Z;
                    index += 3;
                }

                return buffer;
            });
            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, (int)_channelBuffer.DeviceBufferSize * SizeFloat, IntPtr.Zero,
                BufferUsageHint.DynamicDraw);
            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * SizeFloat,
                0);
            GL.EnableVertexAttribArray(0);
            ElementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            //index缓存从一开始就是确定的，所以提前分配
            //在gpu环形缓冲中存在首副本节点，所以index缓存也必须扩充 //留有两个model的裕度
            var indexBuffer = PopulateIndex((int)ChannelWidth, (int)_channelBuffer.SuggestMaxModelCount + 2);
            // var indexBuffer = new uint[] { 0, 2, 1, 3, 3, 2, 2, 4, 3, 5, 5, 4, 4 };
            GL.BufferData(BufferTarget.ElementArrayBuffer, indexBuffer.Length * SizeOfUint, indexBuffer,
                BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            this.IsInitialized = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xLength">横向宽度</param>
        /// <param name="yLength">纵向宽度</param>
        /// <returns></returns>
        private static int CalculateIndexLength(int xLength, int yLength)
        {
            if (yLength < 2)
            {
                return 0;
            }

            int numStripsRequired = yLength - 1;
            int numDegensRequired = 2 * (numStripsRequired - 1);
            int verticesPerStrip = 2 * xLength;
            return (verticesPerStrip * numStripsRequired) + numDegensRequired;
        }

        /// <summary>
        /// populate index buffer data  source code from :https://www.learnopengles.com/tag/triangle-strips/
        /// </summary>
        /// <param name="xLength">横向宽度</param>
        /// <param name="yLength">纵向宽度</param>
        public static uint[] PopulateIndex(int xLength, int yLength)
        {
            var indexData = new uint[CalculateIndexLength(xLength, yLength)];
            var offset = 0;
            for (int y = 0; y < yLength - 1; y++)
            {
                if (y > 0)
                {
                    // Degenerate begin: repeat first vertex
                    indexData[offset++] = (uint)(y * xLength);
                }

                for (int x = 0; x < xLength; x++)
                {
                    // One part of the strip
                    indexData[offset++] = (uint)((y * xLength) + x);
                    indexData[offset++] = (uint)(((y + 1) * xLength) + x);
                }

                if (y < yLength - 2)
                {
                    // Degenerate end: repeat last vertex
                    indexData[offset++] = (uint)((y + 1) * xLength + (xLength - 1));
                }
            }

            return indexData;
        }

        public bool PreviewRender()
        {
            if (_channelBuffer.TryFlush(out var gpuBufferRegions))
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
                foreach (var updateRegion in gpuBufferRegions)
                {
                    GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)(updateRegion.Low * SizeFloat),
                        (IntPtr)(updateRegion.Length * SizeFloat), updateRegion.Data);
                }

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }

            return true;
        }

        public void ApplyDirective(RenderDirective directive)
        {
        }

        public void Render(GlRenderEventArgs args)
        {
            if (_channelBuffer.RecentModelCount < 2)
            {
                return;
            }

            GL.BindVertexArray(VertexArrayObject);
            var drawRegions = _channelBuffer.EffectRegions;
            var firstDrawRegion = drawRegions[0];
            var firstDrawRegionChannelCount = firstDrawRegion.Length / ChannelBufferWidth;
            if (drawRegions.Count == 1)
            {
                var calculateIndexLength =
                    CalculateIndexLength((int)this.ChannelWidth, (int)firstDrawRegionChannelCount);
                var offsetLength = this.ChannelWidth * 2 + 2; //第一排channel+两个退化点，固定值//固定值 
                GL.DrawElements(BeginMode.TriangleStrip, calculateIndexLength, DrawElementsType.UnsignedInt,
                    (int)offsetLength * SizeOfUint);
                //固定值 (int)((drawRegion.Tail / ChannelBufferWidth + ChannelWidth) * uintsize
            }
            else
            {
                if (firstDrawRegionChannelCount >= 2) //小于两个通道不能形成多面体
                {
                    var calculateIndexLength =
                        CalculateIndexLength((int)this.ChannelWidth, (int)firstDrawRegionChannelCount);
                    //ringbuffer对应的抽象indexbuffer里越过的channel数量
                    var channelVirtualOffset = (firstDrawRegion.Tail + 1) / ChannelBufferWidth;
                    //真实indexbuffer越过的channel数量，需要+1算上拷贝的channel
                    var channelOffset = channelVirtualOffset + 1;
                    //由于总是存在退化点，可以认为点位数量=行数*行宽*2+行数*2（退化点）
                    var offset = channelOffset * this.ChannelWidth * 2 + 2 * channelOffset;
                    GL.DrawElements(BeginMode.TriangleStrip, calculateIndexLength,
                        DrawElementsType.UnsignedInt, (int)offset * SizeOfUint);
                }

                var secondRegion = drawRegions[1]; //第二个绘制区域
                //索引从零开始
                var secondRegionChannelCount = secondRegion.Length / ChannelBufferWidth + 1;
                var indexLength = CalculateIndexLength((int)this.ChannelWidth, (int)secondRegionChannelCount);
                GL.DrawElements(BeginMode.TriangleStrip, indexLength, DrawElementsType.UnsignedInt,
                    0);
            }
        }

        public void Resize(PixelSize size)
        {
        }

        public void Uninitialize()
        {
            if (!IsInitialized)
            {
                return;
            }

            GL.DeleteBuffer(ElementBufferObject);
            GL.DeleteBuffer(VertexBufferObject);
            GL.DeleteVertexArray(VertexArrayObject);
            IsInitialized = false;
        }

        public void BindShader(Shader shader)
        {
            this._shader = shader;
        }

        public void Add(IChannel geometry)
        {
            _channelBuffer.SendChange(NotifyCollectionChangedEventArgs<IChannel>.AppendArgs(geometry));
        }

        public void AddRange(IList<IChannel> geometries)
        {
            _channelBuffer.SendChange(NotifyCollectionChangedEventArgs<IChannel>.AppendRangeArgs(geometries));
        }

        public void ResetWith(IList<IChannel> geometries)
        {
            _channelBuffer.SendChange(
                new NotifyCollectionChangedEventArgs<IChannel>(NotifyCollectionChangedAction.Reset, geometries));
        }

        public void ResetWith(IChannel geometry)
        {
            _channelBuffer.SendChange(
                new NotifyCollectionChangedEventArgs<IChannel>(NotifyCollectionChangedAction.Reset, geometry));
        }

        public void Clear()
        {
            _channelBuffer.SendChange(NotifyCollectionChangedEventArgs<IChannel>.ResetArgs);
        }
    }
}