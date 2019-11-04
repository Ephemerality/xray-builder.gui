using SimpleInjector;
using XRayBuilderGUI.Libraries.Bootstrap.Model;

namespace XRayBuilderGUI.DataSources.Amazon.Bootstrap
{
    public class BootstrapAmazon : IBootstrapSegment, IContainerSegment
    {
        public void Register(IBootstrapBuilder builder)
        {
        }

        public void Register(Container container)
        {
            container.Register<IAmazonClient, AmazonClient>(Lifestyle.Singleton);
            container.Register<IAmazonInfoParser, AmazonInfoParser>(Lifestyle.Singleton);
        }
    }
}