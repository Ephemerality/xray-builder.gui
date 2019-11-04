using SimpleInjector;
using XRayBuilder.Core.Libraries.Bootstrap.Model;

namespace XRayBuilder.Core.Libraries.Http.Bootstrap
{
    public sealed class BootstrapHttp : IBootstrapSegment, IContainerSegment
    {
        public void Register(IBootstrapBuilder builder)
        {
        }

        public void Register(Container container)
        {
            container.RegisterSingleton<IHttpClient, HttpClient>();
        }
    }
}