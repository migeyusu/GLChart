using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using RLP.Chart.OpenGL.Renderer;
using Xunit;
using Xunit.Abstractions;

namespace RLP.Chart.Client.UnitTest
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