using SimpleInjector;
using XRayBuilderGUI.Libraries.Bootstrap.Model;
using XRayBuilderGUI.Libraries.SimpleInjector.Extensions;

namespace XRayBuilderGUI.DataSources.Secondary.Bootstrap
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