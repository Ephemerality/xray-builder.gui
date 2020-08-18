using SimpleInjector;
using XRayBuilder.Core.Libraries.Bootstrap.Model;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Logging.Bootstrap;

namespace XRayBuilder.Core.Libraries.Http.Bootstrap
{
    public sealed class BootstrapHttp : IBootstrapSegment, IContainerSegment
    {
        public void Register(IBootstrapBuilder builder)
        {
            builder.Register<BootstrapLogging>();
        }

        public void Register(Container container)
        {
            container.RegisterSingleton<IHttpClient>(() => new HttpClient(container.GetInstance<ILogger>()));
        }
    }
}