using System;
using System.Windows.Forms;
using SimpleInjector;
using XRayBuilderGUI.DataSources.Amazon.Bootstrap;
using XRayBuilderGUI.DataSources.Secondary.Bootstrap;
using XRayBuilderGUI.Extras.BootstrapExtras;
using XRayBuilderGUI.Libraries;
using XRayBuilderGUI.Libraries.Bootstrap.Logic;
using XRayBuilderGUI.Libraries.Http.Bootstrap;
using XRayBuilderGUI.Libraries.Logging.Bootstrap;
using XRayBuilderGUI.Libraries.SimpleInjector.Extensions;
using XRayBuilderGUI.UI;
using XRayBuilderGUI.UI.Bootstrap;
using XRayBuilderGUI.XRay.Bootstrap;

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

            _container.AutoregisterConcreteFromInterface(typeof(IFactory<,>), Lifestyle.Singleton);

            var builder = new BootstrapBuilder(_container);

            builder.Register<BootstrapLogging>();
            builder.Register<BootstrapHttp>();
            builder.Register<BootstrapExtras>();
            builder.Register<BootstrapAmazon>();
            builder.Register<BootstrapSecondary>();
            builder.Register<BootstrapUI>();
            builder.Register<BootstrapXRay>();

            builder.Build();
        }
    }
}