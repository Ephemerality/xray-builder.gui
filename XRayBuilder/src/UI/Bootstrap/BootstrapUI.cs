using SimpleInjector;
using XRayBuilder.Core.Libraries.Bootstrap.Model;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.SimpleInjector.Extensions;
using XRayBuilderGUI.UI.Preview;
using XRayBuilderGUI.UI.Preview.Model;

namespace XRayBuilderGUI.UI.Bootstrap
{
    // ReSharper disable once InconsistentNaming
    public sealed class BootstrapUI : IBootstrapSegment, IContainerSegment
    {
        public void Register(IBootstrapBuilder builder)
        {
        }

        public void Register(Container container)
        {
            container.RegisterTransientIgnore<frmMain>("Disposed by application");
            container.RegisterSingleton<PreviewProviderFactory>();
            container.AutoregisterConcreteFromAbstract<PreviewProvider>(Lifestyle.Singleton);
            container.AutoregisterDisposableTransientConcreteFromInterface<IPreviewForm>("Manually disposed");
            container.RegisterSingleton<ILogger, Logger>();
            container.RegisterTransientIgnore<frmCreateXR>("Disposed by application");
        }
    }
}