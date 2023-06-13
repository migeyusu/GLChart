namespace GLChart.OpenTK.Renderer
{
    public class GPUBufferRegion<TK> where TK : struct
    {
        public long Low;

        public long High;

        public TK[] Data;

        public long Length => High - Low + 1;
    }
}