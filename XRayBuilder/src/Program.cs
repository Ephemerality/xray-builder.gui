﻿using System;
using System.Windows.Forms;
using SimpleInjector;
using XRayBuilder.Core.Bootstrap;
using XRayBuilder.Core.Config;
using XRayBuilder.Core.DataSources.Amazon.Bootstrap;
using XRayBuilder.Core.DataSources.Roentgen.Bootstrap;
using XRayBuilder.Core.DataSources.Secondary.Bootstrap;
using XRayBuilder.Core.Extras.Bootstrap;
using XRayBuilder.Core.Libraries;
using XRayBuilder.Core.Libraries.Bootstrap.Logic;
using XRayBuilder.Core.Libraries.Http.Bootstrap;
using XRayBuilder.Core.Libraries.Logging.Bootstrap;
using XRayBuilder.Core.Libraries.SimpleInjector.Extensions;
using XRayBuilder.Core.Model.Exceptions;
using XRayBuilder.Core.XRay.Bootstrap;
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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Bootstrap();
            }
            // SimpleInjector verification throws an InvalidOperationException with an ActivationException that contains the actual exception
            catch (InvalidOperationException e) when (e.InnerException?.InnerException is InitializationException iEx)
            {
                MessageBox.Show($"Failed to initialize the application:\r\n{iEx.Message}");
            }
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
            builder.Register<BootstrapRoentgen>();
            builder.Register<BootstrapXRayBuilder>();

            _container.RegisterSingleton(() => new XRayBuilderConfig
            {
                UseSubdirectories = Properties.Settings.Default.useSubDirectories,
                BaseOutputDirectory = Properties.Settings.Default.outDir,
                BuildForAndroid = Properties.Settings.Default.android,
                OutputToSidecar = Properties.Settings.Default.outputToSidecar
            });

            builder.Build();
        }
    }
}