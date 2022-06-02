using OpenTK;

namespace RLP.Chart.OpenGL.Renderer
{
    /// <summary>
    /// 绘制指令
    /// </summary>
    public class RenderDirective
    {
        

        public  Matrix4 Transform { get; }

        public RenderDirective(Matrix4 transform)
        {
            Transform = transform;
        }
    }
}