using SimpleInjector;
using XRayBuilder.Core.Libraries.Bootstrap.Model;
using XRayBuilder.Core.Libraries.Http.Bootstrap;

namespace XRayBuilder.Core.DataSources.Amazon.Bootstrap
{
    public sealed class BootstrapAmazon : IBootstrapSegment, IContainerSegment
    {
        public void Register(IBootstrapBuilder builder)
        {
            builder.Register<BootstrapHttp>();
        }

        public void Register(Container container)
        {
            container.RegisterSingleton<IAmazonClient, AmazonClient>();
            container.RegisterSingleton<IAmazonInfoParser, AmazonInfoParser>();
        }
    }
}