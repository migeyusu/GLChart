using RLP.Chart.OpenGL.Renderer;

namespace RLP.Chart.OpenGL.Abstraction
{
    /// <summary>
    /// 绑定着色器的渲染器
    /// </summary>
    public interface IShaderRendererItem : IRendererItem
    {

        void BindShader(Shader shader);
    }
}