using SimpleInjector;
using XRayBuilder.Core.Libraries.Bootstrap.Model;

namespace XRayBuilder.Core.Libraries.Logging.Bootstrap
{
    public sealed class BootstrapLogging : IBootstrapSegment, IContainerSegment
    {
        public void Register(IBootstrapBuilder builder)
        {
        }

        public void Register(Container container)
        {
            container.RegisterSingleton<ILogger, Logger>();
        }
    }
}