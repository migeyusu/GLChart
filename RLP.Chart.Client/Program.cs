using System;
using System.Windows;

namespace RLP.Chart.Client
{ 
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var application = new Application();
            application.Run(new RenderTestWindow());
        }
    }
}