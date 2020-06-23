using SimpleInjector;
using XRayBuilder.Core.Libraries.Bootstrap.Model;
using XRayBuilder.Core.Logic;

namespace XRayBuilder.Core.Bootstrap
{
    public sealed class BootstrapXRayBuilder : IBootstrapSegment, IContainerSegment
    {
        public void Register(IBootstrapBuilder builder)
        {
        }

        public void Register(Container container)
        {
            container.RegisterSingleton<IDirectoryService, DirectoryService>();
        }
    }
}