using SimpleInjector;
using XRayBuilderGUI.Libraries.Bootstrap.Model;

namespace XRayBuilderGUI.Libraries.Http.Bootstrap
{
    public sealed class BootstrapHttp : IBootstrapSegment, IContainerSegment
    {
        public void Register(IBootstrapBuilder builder)
        {
        }

        public void Register(Container container)
        {
            container.Register<IHttpClient, HttpClient>(Lifestyle.Singleton);
        }
    }
}