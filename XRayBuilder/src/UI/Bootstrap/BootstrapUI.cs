using SimpleInjector;
using XRayBuilderGUI.Libraries.Bootstrap.Model;
using XRayBuilderGUI.Libraries.SimpleInjector.Extensions;
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
            container.Register<PreviewProviderFactory>(Lifestyle.Singleton);
            container.AutoregisterConcreteFromAbstract<PreviewProvider>(Lifestyle.Singleton);
            container.AutoregisterDisposableTransientConcreteFromInterface<IPreviewForm>("Manually disposed");
        }
    }
}