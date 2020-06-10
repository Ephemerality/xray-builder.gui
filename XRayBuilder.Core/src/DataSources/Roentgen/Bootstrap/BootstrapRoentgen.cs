using SimpleInjector;
using XRayBuilder.Core.DataSources.Roentgen.Logic;
using XRayBuilder.Core.Libraries.Bootstrap.Model;

namespace XRayBuilder.Core.DataSources.Roentgen.Bootstrap
{
    public class BootstrapRoentgen : IBootstrapSegment, IContainerSegment
    {
        public void Register(IBootstrapBuilder builder)
        {
        }

        public void Register(Container container)
        {
            container.RegisterSingleton<IRoentgenClient, RoentgenClient>();
        }
    }
}