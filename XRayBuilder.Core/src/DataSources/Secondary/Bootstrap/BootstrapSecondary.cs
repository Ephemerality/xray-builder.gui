using SimpleInjector;
using XRayBuilder.Core.DataSources.Amazon.Bootstrap;
using XRayBuilder.Core.DataSources.Roentgen.Bootstrap;
using XRayBuilder.Core.Libraries.Bootstrap.Model;
using XRayBuilder.Core.Libraries.Http.Bootstrap;
using XRayBuilder.Core.Libraries.SimpleInjector.Extensions;
using XRayBuilder.Core.XRay.Bootstrap;

namespace XRayBuilder.Core.DataSources.Secondary.Bootstrap
{
    public sealed class BootstrapSecondary : IBootstrapSegment, IContainerSegment
    {
        public void Register(IBootstrapBuilder builder)
        {
            builder.Register<BootstrapXRay>();
            builder.Register<BootstrapHttp>();
            builder.Register<BootstrapAmazon>();
            builder.Register<BootstrapRoentgen>();
        }

        public void Register(Container container)
        {
            container.AutoregisterConcreteFromInterface<ISecondarySource>(Lifestyle.Singleton);
            container.RegisterSingleton<SecondaryDataSourceFactory>();
        }
    }
}