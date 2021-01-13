using SimpleInjector;
using XRayBuilder.Core.Config;
using XRayBuilder.Core.Libraries.Bootstrap.Model;

namespace XRayBuilderGUI.Config
{
    public sealed class BootstrapConfig : IBootstrapSegment, IContainerSegment
    {
        public void Register(IBootstrapBuilder builder)
        {
        }

        public void Register(Container container)
        {
            container.RegisterSingleton<IXRayBuilderConfig, XRayBuilderConfig>();
        }
    }
}