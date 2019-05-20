using System;
using System.Windows.Forms;
using SimpleInjector;
using SimpleInjector.Diagnostics;
using XRayBuilderGUI.DataSources.Amazon;
using XRayBuilderGUI.DataSources.Secondary;
using XRayBuilderGUI.UI;
using XRayBuilderGUI.UI.Preview;
using XRayBuilderGUI.UI.Preview.Logic;

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
            _container.Register<frmMain>();
            var registration = _container.GetRegistration(typeof(frmMain)).Registration;
            registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, "Disposed by application");

            _container.Register<IHttpClient, HttpClient>(Lifestyle.Singleton);
            _container.Register<IAmazonClient, AmazonClient>(Lifestyle.Singleton);
            _container.Register<IAuthorProfileGenerator, AuthorProfileGenerator>(Lifestyle.Singleton);

            // TODO: Autoregister Factory<>, SecondaryDataSources, PreviewProviders
            _container.Register<SecondaryDataSourceFactory>(Lifestyle.Singleton);
            _container.Register<Shelfari>(Lifestyle.Singleton);
            _container.Register<Goodreads>(Lifestyle.Singleton);

            _container.Register<PreviewProviderFactory>(Lifestyle.Singleton);
            _container.Register<PreviewProvider.PreviewProviderAuthorProfile>(Lifestyle.Singleton);
            _container.Register<PreviewProvider.PreviewProviderEndActions>(Lifestyle.Singleton);
            _container.Register<PreviewProvider.PreviewProviderStartActions>(Lifestyle.Singleton);
            _container.Register<PreviewProvider.PreviewProviderXRay>(Lifestyle.Singleton);
            _container.Register<frmPreviewAP>();
            _container.Register<frmPreviewEA>();
            _container.Register<frmPreviewSA>();
            _container.Register<frmPreviewXR>();
            registration = _container.GetRegistration(typeof(frmPreviewAP)).Registration;
            registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, "Manually disposed");
            registration = _container.GetRegistration(typeof(frmPreviewEA)).Registration;
            registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, "Manually disposed");
            registration = _container.GetRegistration(typeof(frmPreviewSA)).Registration;
            registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, "Manually disposed");
            registration = _container.GetRegistration(typeof(frmPreviewXR)).Registration;
            registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, "Manually disposed");

            _container.Verify();
        }
    }
}