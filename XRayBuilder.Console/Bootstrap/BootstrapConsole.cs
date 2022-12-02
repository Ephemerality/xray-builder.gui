using SimpleInjector;
using XRayBuilder.Console.Logic;
using XRayBuilder.Core.Bootstrap;
using XRayBuilder.Core.Config;
using XRayBuilder.Core.DataSources.Amazon.Bootstrap;
using XRayBuilder.Core.DataSources.Roentgen.Bootstrap;
using XRayBuilder.Core.DataSources.Secondary.Bootstrap;
using XRayBuilder.Core.Libraries.Bootstrap.Model;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.XRay.Bootstrap;

namespace XRayBuilder.Console.Bootstrap
{
    public class BootstrapConsole : IBootstrapSegment, IContainerSegment
    {
        public void Register(IBootstrapBuilder builder)
        {
            builder.Register<BootstrapAmazon>();
            builder.Register<BootstrapSecondary>();
            builder.Register<BootstrapXRay>();
            builder.Register<BootstrapRoentgen>();
            builder.Register<BootstrapXRayBuilder>();
        }

        public void Register(Container container)
        {
            container.RegisterSingleton<XRay>();
            container.RegisterSingleton<IProgressBar, ProgressConsole>();
            container.RegisterSingleton<ILogger, ConsoleLogger>();
            container.RegisterInstance(new ApplicationConfig
            {
                Unattended = true
            });
        }
    }
}