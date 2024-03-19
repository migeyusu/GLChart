using System;
using System.Windows;

namespace GLChart.Samples
{ 
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var application = new App();
            application.Run(new ChartTestWindow());
            application.Shutdown();
        }
    }
}