using SimpleInjector;
using XRayBuilderGUI.Libraries.Bootstrap.Model;
using XRayBuilderGUI.Libraries.SimpleInjector.Extensions;
using XRayBuilderGUI.XRay.Logic;
using XRayBuilderGUI.XRay.Logic.Aliases;
using XRayBuilderGUI.XRay.Logic.Chapters;
using XRayBuilderGUI.XRay.Logic.Export;
using XRayBuilderGUI.XRay.Logic.Terms;

namespace XRayBuilderGUI.XRay.Bootstrap
{
    public sealed class BootstrapXRay : IBootstrapSegment, IContainerSegment
    {
        public void Register(IBootstrapBuilder builder)
        {
        }

        public void Register(Container container)
        {
            container.AutoregisterConcreteFromInterface<IXRayExporter>(Lifestyle.Singleton);
            container.Register<IXRayService, XRayService>(Lifestyle.Singleton);
            container.Register<ITermsService, TermsService>(Lifestyle.Singleton);
            container.Register<IAliasesService, AliasesService>(Lifestyle.Singleton);
            container.Register<IAliasesRepository, AliasesRepository>(Lifestyle.Singleton);
            container.Register<ChaptersService>(Lifestyle.Singleton);
            container.Register<IPreviewDataExporter, PreviewDataExporter>(Lifestyle.Singleton);
        }
    }
}