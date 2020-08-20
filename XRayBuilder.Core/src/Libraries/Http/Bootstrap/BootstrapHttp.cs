using SimpleInjector;
using XRayBuilder.Core.Libraries.Bootstrap.Model;
using XRayBuilder.Core.Libraries.Logging;

namespace XRayBuilder.Core.Libraries.Http.Bootstrap
{
    public sealed class BootstrapHttp : IBootstrapSegment, IContainerSegment
    {
        public void Register(IBootstrapBuilder builder)
        {
        }

        public void Register(Container container)
        {
            container.RegisterSingleton<IHttpClient>(() => new HttpClient(container.GetInstance<ILogger>()));
        }
    }
}