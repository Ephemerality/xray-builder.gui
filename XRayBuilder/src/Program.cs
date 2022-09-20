using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using SimpleInjector;
using XRayBuilder.Core.Bootstrap;
using XRayBuilder.Core.DataSources.Amazon.Bootstrap;
using XRayBuilder.Core.DataSources.Roentgen.Bootstrap;
using XRayBuilder.Core.DataSources.Secondary.Bootstrap;
using XRayBuilder.Core.Extras.Bootstrap;
using XRayBuilder.Core.Libraries;
using XRayBuilder.Core.Libraries.Bootstrap.Logic;
using XRayBuilder.Core.Libraries.Http.Bootstrap;
using XRayBuilder.Core.Libraries.Language.Bootstrap;
using XRayBuilder.Core.Libraries.Language.Localization;
using XRayBuilder.Core.Libraries.SimpleInjector.Extensions;
using XRayBuilder.Core.Model.Exceptions;
using XRayBuilder.Core.XRay.Bootstrap;
using XRayBuilderGUI.Config;
using XRayBuilderGUI.Localization.Main;
using XRayBuilderGUI.Properties;
using XRayBuilderGUI.UI;
using XRayBuilderGUI.UI.Bootstrap;

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
            if (Environment.OSVersion.Version.Major >= 6)
                SetProcessDPIAware();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Bootstrap();
            }
            // SimpleInjector verification throws an InvalidOperationException with an ActivationException that contains the actual exception
            catch (InvalidOperationException e) when (e.InnerException?.InnerException is InitializationException iEx)
            {
                MessageBox.Show($@"{MainStrings.InitializeFailed}:\r\n{iEx.Message}");
            }

            // Use the saved language if it's available, otherwise fall back on the system language if supported, otherwise default to English
            var languageFactory = _container.GetInstance<LanguageFactory>();
            var language = languageFactory.Get(Settings.Default.Language)
                    ?? languageFactory.GetWindowsLanguage()
                    ?? languageFactory.Get(LanguageFactory.Enum.English);

            if (language.Language.ToString() != Settings.Default.Language)
            {
                Settings.Default.Language = language.Language.ToString();
                Settings.Default.Save();
            }

            language.CultureInfo.SetAsThreadCulture();

            // Hack to load the right DLL until the actual issue resolving the assembly is figured out
            AppDomain.CurrentDomain.AssemblyResolve += (_, args)
                => args.Name.StartsWith("Amazon.IonDotnet")
                    ? Assembly.LoadFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Amazon.IonDotnet.Ephemerality.dll"))
                    : null;

            Application.Run(_container.GetInstance<frmMain>());
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        private static void Bootstrap()
        {
            _container = new Container();

            // TODO this doesn't work due to generic typing on interfaces not working with IsAssignableFrom
            _container.AutoregisterConcreteFromInterface(typeof(IFactory<,>), Lifestyle.Singleton);

            var builder = new BootstrapBuilder(_container);

            builder.Register<BootstrapHttp>();
            builder.Register<BootstrapExtras>();
            builder.Register<BootstrapAmazon>();
            builder.Register<BootstrapSecondary>();
            builder.Register<BootstrapUI>();
            builder.Register<BootstrapXRay>();
            builder.Register<BootstrapRoentgen>();
            builder.Register<BootstrapXRayBuilder>();
            builder.Register<BootstrapConfig>();
            builder.Register<BootstrapLanguage>();

            builder.Build();
        }
    }
}