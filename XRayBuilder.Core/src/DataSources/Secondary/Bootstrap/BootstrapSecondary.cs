using SimpleInjector;
using XRayBuilder.Core.Libraries.Bootstrap.Model;
using XRayBuilder.Core.Libraries.SimpleInjector.Extensions;

namespace XRayBuilder.Core.DataSources.Secondary.Bootstrap
{
    public sealed class BootstrapSecondary : IBootstrapSegment, IContainerSegment
    {
        public void Register(IBootstrapBuilder builder)
        {
        }

        public void Register(Container container)
        {
            container.AutoregisterConcreteFromInterface<ISecondarySource>(Lifestyle.Singleton);
        }
    }
}