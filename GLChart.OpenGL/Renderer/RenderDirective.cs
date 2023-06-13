using OpenTK.Mathematics;

namespace RLP.Chart.OpenGL.Renderer
{
    /// <summary>
    /// 绘制指令
    /// </summary>
    public class RenderDirective
    {
        public RenderDirective()
        {
        }
    }

    public class RenderDirective2D : RenderDirective
    {
        public Matrix4 Transform { get; set; }
    }

    public class RenderDirective3D : RenderDirective
    {
        /// <summary>
        /// 当前设定的坐标区域
        /// </summary>
        public Region3D Region3D { get; set; }

        /// <summary>
        /// 投影矩阵
        /// </summary>
        public Matrix4 Projection { get; set; }

        /// <summary>
        /// 视图矩阵
        /// </summary>
        public Matrix4 View { get; set; }
    }
}