using SimpleInjector;
using XRayBuilderGUI.Libraries.Bootstrap.Model;

namespace XRayBuilderGUI.Libraries.Logging.Bootstrap
{
    public sealed class BootstrapLogging : IBootstrapSegment, IContainerSegment
    {
        public void Register(IBootstrapBuilder builder)
        {
        }

        public void Register(Container container)
        {
            container.Register<ILogger, Logger>(Lifestyle.Singleton);
        }
    }
}