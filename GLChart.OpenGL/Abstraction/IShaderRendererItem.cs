using GLChart.Core.Renderer;

namespace GLChart.Core.Abstraction
{
    /// <summary>
    /// 可绑定着色器的渲染器
    /// </summary>
    public interface IShaderRendererItem : IRendererItem
    {
        void BindShader(Shader shader);
    }
}