using SimpleInjector;
using XRayBuilderGUI.Libraries.Bootstrap.Model;

namespace XRayBuilderGUI.DataSources.Amazon.Bootstrap
{
    public sealed class BootstrapAmazon : IBootstrapSegment, IContainerSegment
    {
        public void Register(IBootstrapBuilder builder)
        {
        }

        public void Register(Container container)
        {
            container.RegisterSingleton<IAmazonClient, AmazonClient>();
            container.RegisterSingleton<IAmazonInfoParser, AmazonInfoParser>();
        }
    }
}