using System;
using System.Windows.Forms;
using SimpleInjector;
using XRayBuilderGUI.DataSources.Amazon;
using XRayBuilderGUI.DataSources.Secondary;
using XRayBuilderGUI.Extras.AuthorProfile;
using XRayBuilderGUI.Libraries.Http;
using XRayBuilderGUI.Libraries.Logging;
using XRayBuilderGUI.Libraries.SimpleInjector.Extensions;
using XRayBuilderGUI.UI;
using XRayBuilderGUI.UI.Preview;
using XRayBuilderGUI.UI.Preview.Logic;
using XRayBuilderGUI.XRay.Logic;
using XRayBuilderGUI.XRay.Logic.Aliases;
using XRayBuilderGUI.XRay.Logic.Export;
using XRayBuilderGUI.XRay.Logic.Terms;
using XRayBuilderGUI.XRay.Model.Export;

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
            _container.RegisterTransientIgnore<frmMain>("Disposed by application");

            _container.Register<IHttpClient, HttpClient>(Lifestyle.Singleton);
            _container.Register<IAmazonClient, AmazonClient>(Lifestyle.Singleton);
            _container.Register<IAuthorProfileGenerator, AuthorProfileGenerator>(Lifestyle.Singleton);

            // TODO: Figure out autoregister for Factory<,>
            _container.Register<SecondaryDataSourceFactory>(Lifestyle.Singleton);
            _container.AutoregisterConcreteFromInterface<ISecondarySource>(Lifestyle.Singleton);
            _container.Register<XRayExporterFactory>(Lifestyle.Singleton);
            _container.AutoregisterConcreteFromInterface<IXRayExporter>(Lifestyle.Singleton);

            _container.Register<IXRayService, XRayService>(Lifestyle.Singleton);
            _container.Register<ITermsService, TermsService>(Lifestyle.Singleton);
            _container.Register<IAliasesService, AliasesService>(Lifestyle.Singleton);

            _container.Register<IPreviewDataExporter, PreviewDataExporter>(Lifestyle.Singleton);

            _container.Register<PreviewProviderFactory>(Lifestyle.Singleton);
            _container.AutoregisterConcreteFromAbstract<PreviewProvider>(Lifestyle.Singleton);
            _container.AutoregisterDisposableTransientConcreteFromInterface<IPreviewForm>("Manually disposed");

            _container.Register<IAmazonInfoParser, AmazonInfoParser>(Lifestyle.Singleton);

            _container.Register<IAliasesRepository, AliasesRepository>();

            _container.Verify();
        }
    }
}