using System.Text;
using GLChart.WPF.Render.Renderer;
using Xunit;
using Xunit.Abstractions;

namespace GLChart.Samples.UnitTest
{
    public class Channel
    {
        private ITestOutputHelper _output;

        public Channel(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Fact]
        public void CreateIndexBuffer()
        {
            var populateIndex = ChannelRenderer.PopulateIndex(5, 3);
            var stringBuilder = new StringBuilder();
            foreach (var s in populateIndex)
            {
                stringBuilder.Append(s);
                stringBuilder.Append(',');
            }

            _output.WriteLine(stringBuilder.ToString());
        }
    }
}