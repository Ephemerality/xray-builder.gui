using System;
using System.Windows.Forms;
using SimpleInjector;
using XRayBuilderGUI.UI;

namespace XRayBuilderGUI
{
    internal static class Program
    {
        private static Container _container;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Bootstrap();
            Application.Run(_container.GetInstance<frmMain>());
        }

        private static void Bootstrap()
        {
            _container = new Container();

            _container.Register<ILogger, Logger>(Lifestyle.Singleton);
            _container.Register<frmMain>(Lifestyle.Singleton);

            _container.Verify();
        }
    }
}