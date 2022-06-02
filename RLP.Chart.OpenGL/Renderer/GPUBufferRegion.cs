namespace RLP.Chart.OpenGL.Renderer
{
    /// <summary>
    /// 表示一段GPU缓冲区域
    /// </summary>
    public struct GPUBufferRegion
    {
        public int Low;

        public int High;

        public float[] Floats;

        public int Length => High - Low + 1;
    }

    public class GPUBufferRegion<TK> where TK : struct
    {
        public long Low;

        public long High;

        public TK[] Data;

        public long Length => High - Low + 1;
    }
}