using SimpleInjector;
using XRayBuilder.Core.DataSources.Roentgen.Logic;
using XRayBuilder.Core.Libraries.Bootstrap.Model;
using XRayBuilder.Core.Libraries.Http.Bootstrap;

namespace XRayBuilder.Core.DataSources.Roentgen.Bootstrap
{
    public class BootstrapRoentgen : IBootstrapSegment, IContainerSegment
    {
        public void Register(IBootstrapBuilder builder)
        {
            builder.Register<BootstrapHttp>();
        }

        public void Register(Container container)
        {
            container.RegisterSingleton<IRoentgenClient, RoentgenClient>();
        }
    }
}